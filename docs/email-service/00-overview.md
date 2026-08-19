# Email Service — Overview

## Why this exists

The platform generates OTPs for email verification and password reset (the two
`EnumOtpPurpose` values with live creation paths — `TwoFactorAuthentication`
and `AccountRecovery` are declared but never created), and the auth use cases
(`SignUp`, `ForgotPassword`, `ResendOtp`, `VerifyOtp`, …) create and persist those
codes — but **nothing delivers them**. There is no email sender, no SMTP
configuration, no provider SDK anywhere in the codebase. Today an OTP is only
observable by reading the `authentication.otps` table.

On top of closing that gap, the product needs recurring email capabilities:

- **Transactional** — OTP delivery, password-reset confirmations, login alerts,
  welcome emails.
- **Marketing / engagement** — newsletter subscription with double opt-in,
  unsubscribe links, and future campaign sends.

## Non-negotiable requirement: swappable provider

The concrete email provider (SMTP relay, Resend, SendGrid, Mailgun, SES, …) must
be replaceable **without touching any business code**. The architecture achieves
this the same way the codebase already isolates Cloudinary behind
`ICloudinaryService` and the Odesli API behind `IStreamingLinkResolutionService`:

- Business code (Identity handlers, newsletter use cases) depends only on an
  application-layer port.
- Each provider is one adapter class in `Infrastructure/Services/`, selected by
  the `EMAIL_PROVIDER` environment variable at startup.
- Adding a provider = one new adapter + one registration branch + env vars.
  Nothing else changes.

## Shape of the solution

A new **Mailer module** (`src/Modules/Mailer/`) following the existing modular
monolith layout (Domain / Application / Infrastructure, `BaseModule`
registration, own PostgreSQL schema `mailer`). It owns:

| Concern | Where |
| --- | --- |
| `EmailMessage` model + `IEmailSender` transport port | `Application/Shared` |
| `IMailer` high-level port consumed by other modules | `Application/Shared` |
| Localized templates (subject/html/text per event) | `Application/Templates` |
| Outbox persistence + background dispatch with retry | `Domain` + `Infrastructure` |
| Provider adapters (SMTP first, HTTP APIs later) | `Infrastructure/Services` |
| Newsletter subscribers + subscribe/confirm/unsubscribe endpoints | `Domain` + `Application/Newsletter` |

Delivery is **outbox-first**: enqueuing an email commits atomically with the
business change that caused it, and a hosted background service performs the
actual provider call with retry/backoff. A provider outage can never fail a
signup, and a crashed process can never lose an accepted email.

## Local development

Docker gets one new dev-only service: **Mailpit** (SMTP sink with a web UI).
Every email the API sends locally lands in its inbox at `http://localhost:8025`
— no real provider account needed to develop or demo any flow. See spec 08.

## Beyond the identity flows — the full trigger audit

A deep sweep of the backend and frontend
([01-email-triggers-audit.md](01-email-triggers-audit.md)) found the gap is
much wider than OTP delivery. The standouts: every commerce endpoint is
admin-only and B2B customers never log in, so email is the **only** channel
to a paying customer — yet invoices, receipts, payment rejections and
force-unpromote reasons are all written into a void; an email-address change
notifies nobody (and requires no re-verification); and moderator notes on
community submissions are unreadable by the people they're written for.

Three consumer waves are specced for implementation now: account security
(spec 11), commerce customers (spec 12), engagement (spec 13). Community
contribution outcomes are catalogued in the audit and parked for a future
spec.

## Reading order

Work through [specs/00-index.md](specs/00-index.md) top to bottom. Specs 01–05
build the module core (skeleton → message model/ports → provider adapters →
templates → outbox). Specs 06–07 wire the first consumers (identity events,
newsletter). Specs 08–10 cover configuration/docker, testing, and the core
verification sweep. Specs 11–13 add the audit-driven consumer waves: account
security, commerce customers, engagement.
