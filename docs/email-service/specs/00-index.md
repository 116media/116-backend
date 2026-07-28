# Email Service — Backend Implementation Specs

Read [../00-overview.md](../00-overview.md) first for the *why* and the
current-state audit. This index is the *how* — start here, work in the order
below.

| # | File | Covers |
| --- | --- | --- |
| 01 | [01-mailer-module-skeleton.md](01-mailer-module-skeleton.md) | `Mailer` module folders, `MailerModule` registration, `mailer` schema, `MailerDbContext` |
| 02 | [02-email-message-model-and-ports.md](02-email-message-model-and-ports.md) | `EmailMessage`, `EmailRecipient`, `IEmailSender` transport port, `IMailer` application port |
| 03 | [03-provider-adapters-and-selection.md](03-provider-adapters-and-selection.md) | `SmtpEmailSender` (MailKit), `ResendEmailSender` (typed HTTP client), `EMAIL_PROVIDER` selection, add-a-provider checklist |
| 04 | [04-templates-and-localization.md](04-templates-and-localization.md) | Template catalog, token substitution, `.resx` localization (neutral/en/fr), subject + html + text |
| 05 | [05-outbox-reliability-and-dispatch.md](05-outbox-reliability-and-dispatch.md) | `OutboxEmailEntity`, atomic enqueue, `OutboxEmailDispatcher` hosted service, retry/backoff, statuses |
| 06 | [06-identity-event-emails.md](06-identity-event-emails.md) | OTP delivery for the live `EnumOtpPurpose` flows, welcome email, login alert |
| 07 | [07-newsletter-subscriptions.md](07-newsletter-subscriptions.md) | `NewsletterSubscriberEntity`, double opt-in, tokenized unsubscribe, public + admin endpoints |
| 08 | [08-configuration-docker-and-environments.md](08-configuration-docker-and-environments.md) | Env vars, `.env.template` additions, Mailpit docker service, per-environment provider matrix |
| 09 | [09-testing-strategy.md](09-testing-strategy.md) | Unit/integration split per the testing rulebook, `StubEmailSender`, loopback adapter tests |
| 10 | [10-verification-checklist.md](10-verification-checklist.md) | Full build/test/manual sweep across specs 01–09 |
| 11 | [11-account-security-emails.md](11-account-security-emails.md) | Password/email/session/role security notifications in Identity |
| 12 | [12-commerce-customer-emails.md](12-commerce-customer-emails.md) | Invoice, receipt, payment-rejected, cancellations, force-unpromote, fulfilment, shoot dates |
| 13 | [13-engagement-emails.md](13-engagement-emails.md) | Comment-reply notification; deferred-engagement scope record |

Specs 11–13 come from the full trigger audit at
[../01-email-triggers-audit.md](../01-email-triggers-audit.md), which also
parks the community-contribution outcomes (lyrics submissions/corrections,
artist verification) as a future spec.

## Global progress

- [x] 01 — Mailer module skeleton
- [x] 02 — Email message model and ports
- [x] 03 — Provider adapters and selection
- [x] 04 — Templates and localization
- [x] 05 — Outbox, reliability and dispatch
- [x] 06 — Identity event emails
- [x] 07 — Newsletter subscriptions
- [x] 08 — Configuration, docker and environments
- [x] 09 — Testing strategy
- [ ] 10 — Verification (core) — build/suites verified; manual Mailpit sweep and provider-swap drill pending
- [x] 11 — Account security emails
- [x] 12 — Commerce customer emails
- [x] 13 — Engagement emails

## Ground rules (apply to every spec)

- Business code never references a concrete provider — only `IMailer` (or, for
  the dispatcher, `IEmailSender`). Swapping providers must remain a
  configuration change.
- Email content is localized through the existing `.resx` pattern; recipient
  language follows the request culture that triggered the email, falling back
  to the neutral culture.
- Every new endpoint follows the Carter + MetaField + rate-limit + validator
  conventions already used by the Identity and Content modules.
- Tests follow [docs/testing/00-unit-vs-integration-rules.md](../../testing/00-unit-vs-integration-rules.md)
  — the provider adapter is integration-tested over loopback like the Odesli
  adapter, and the API host stubs `IEmailSender` like Cloudinary.
