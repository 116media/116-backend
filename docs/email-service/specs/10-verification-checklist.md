# Spec 10 — Verification Checklist

The final sweep, once specs 01–09 are implemented and individually checked.
Verify against the **current** codebase — do not trust earlier phase notes;
fix regressions before ticking.

## Build and suites

- [x] `dotnet build` clean, zero warnings introduced
- [x] `dotnet csharpier .` produces no diff
- [x] Full unit suite green (`dotnet test tests/Unit`)
- [x] Full integration suite green (`dotnet test tests/Integration`)

## Architecture invariants

- [ ] `grep -r "SmtpEmailSender\|ResendEmailSender" src/` matches only
      `Infrastructure/Services/` and `MailerModule` — no business code
      references an adapter
- [ ] `grep -r "MailKit" src/` matches only the SMTP adapter
- [ ] No handler/factory outside the Mailer module references `IEmailSender` —
      consumers use `IMailer` exclusively
- [x] ~~`IMailer` implementation contains no `CommitAsync` call~~ superseded
      by spec 02's atomicity revision: `OutboxMailer` commits its own context
- [ ] Unknown `EMAIL_PROVIDER` and missing `EMAIL_FROM_ADDRESS` fail at boot

## Functional sweep (dev, against Mailpit)

- [ ] Public signup → verification OTP email in Mailpit, both HTML and text
      parts, correct culture
- [ ] Forgot password (public + admin) → reset OTP email; unknown email → 200
      and **no** email
- [ ] Resend OTP → second email with the same purpose template
- [ ] Verify OTP → welcome email
- [ ] Newsletter subscribe → confirm email → click confirm URL → welcome email
      with working unsubscribe URL → click unsubscribe → status
      `Unsubscribed`, links idempotent on re-click
- [ ] French request (`Accept-Language: fr`) produces French subject and body
- [ ] Stop Mailpit, trigger an OTP → outbox row retries with backoff; start
      Mailpit → email drains and row flips to `Sent`

## Provider swap drill (the requirement, proven)

- [ ] Switch dev env to `EMAIL_PROVIDER=resend` with a dummy key: boot passes,
      sends fail as transient, outbox retains rows — then switch back to
      `smtp`: the same rows drain through Mailpit untouched
- [ ] Confirm the add-a-provider recipe in spec 03 still matches reality
      (adapter + switch branch + env + tests, nothing else)

## Documentation closure

- [ ] Every spec's own checklist fully ticked, with any deviations recorded in
      that spec
- [ ] [00-index.md](00-index.md) global progress all `[x]`
- [ ] `.env.template`, `CLAUDE.md` env/module tables, and the docker docs
      mention the Mailer module and Mailpit
