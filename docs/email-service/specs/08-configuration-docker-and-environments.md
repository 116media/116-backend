# Spec 08 — Configuration, Docker and Environments

## Goal

Every knob the email service reads, in one table; the dev docker setup that
makes local email fully offline; and the per-environment provider matrix.

## Environment variables

Append to `.env.template` (grouped with a blank line, no separator comments):

| Variable | Default | Used by |
| --- | --- | --- |
| `EMAIL_PROVIDER` | `smtp` | adapter selection switch (spec 03) |
| `EMAIL_FROM_ADDRESS` | — (required) | all adapters |
| `EMAIL_FROM_NAME` | `116` | all adapters |
| `SMTP_HOST` | `localhost` | `SmtpEmailSender` |
| `SMTP_PORT` | `1025` | `SmtpEmailSender` |
| `SMTP_USERNAME` | empty ⇒ no AUTH | `SmtpEmailSender` |
| `SMTP_PASSWORD` | empty ⇒ no AUTH | `SmtpEmailSender` |
| `SMTP_USE_STARTTLS` | `false` | `SmtpEmailSender` |
| `RESEND_API_KEY` | — | `ResendEmailSender` (only when provider is `resend`) |
| `FRONTEND_BASE_URL` | `http://localhost:3000` | newsletter confirm/unsubscribe links (spec 07) |

Boot-time validation: when `EMAIL_PROVIDER=resend` and `RESEND_API_KEY` is
missing, or `EMAIL_FROM_ADDRESS` is missing entirely, fail startup with a
clear message — same philosophy as the unknown-provider guard.

## Docker — yes, one dev-only service

Local development needs an SMTP sink, not a real provider. Add **Mailpit** to
`docker-compose.override.yml` (dev overlay only — it must never ship in the
production compose file):

```yaml
    116_mailpit:
        image: axllent/mailpit:latest
        container_name: 116_mailpit
        ports:
            - "1025:1025"   # SMTP — the API sends here
            - "8025:8025"   # Web UI — http://localhost:8025
```

- The API's dev env uses `EMAIL_PROVIDER=smtp`, `SMTP_HOST=116_mailpit`
  (in-network) or `localhost` (running the API on the host), port `1025`.
- Every OTP/welcome/newsletter email is then inspectable in the Mailpit UI —
  including HTML rendering and the text part — with zero external accounts.
- Mailpit is also the manual-verification tool for spec 10's sweep.

## Per-environment matrix

| Environment | Provider | Notes |
| --- | --- | --- |
| Local dev | `smtp` → Mailpit | offline, inspect at `:8025` |
| CI / integration tests | none — `IEmailSender` stubbed (spec 09) | outbox rows asserted instead of deliveries |
| Staging | `smtp` → Mailpit or a sandboxed relay | keeps staging from emailing real users |
| Production | `resend` (or any future adapter) | verified sending domain, real key |

## Runbook notes

- **Provider outage**: outbox rows accumulate as `Pending` with growing
  `next_attempt_at`; they drain automatically when the provider recovers.
  Nothing to do unless rows hit `Failed`.
- **Re-queue failed rows** after fixing a root cause:
  `UPDATE mailer.outbox_emails SET status = 'Pending', attempt_count = 0, next_attempt_at = now() WHERE status = 'Failed';`
- **Switching provider in prod**: set the new `EMAIL_PROVIDER` + its env vars,
  restart. Pending outbox rows drain through the new provider — the outbox is
  provider-agnostic by construction.

## Checklist

- [x] `.env.template` updated with the table above
- [x] Boot-time validation for required/conditional vars
- [x] Mailpit service in `docker-compose.override.yml` only
- [x] Dev `.env` values documented (`SMTP_HOST`, port 1025)
- [x] Runbook statements verified against the real schema names

## Implementation notes

- Mailpit ships in `docker-compose.override.yml` only, with the API pointed
  at `116_mailpit:1025` in-network; `.env.template` gained the full variable
  block including `FRONTEND_BASE_URL`.
- Boot-time validation is implemented for the provider name (unknown value
  throws in `MailerModule`); `EMAIL_FROM_ADDRESS` is validated at first use
  inside the adapters rather than at boot — both adapters throw a clear
  `InvalidOperationException` naming the variable.
