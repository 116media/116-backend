# Spec 06 — Identity Event Emails

## Goal

Close the delivery gap: every flow that creates an OTP actually emails it, and
the two non-OTP notifications (welcome, login alert) fire from their events.
This spec touches the Identity module — the only consumer-side spec besides
the newsletter.

## Where the sends hook in

The OTP factories are the single choke points that create OTP rows, so they
are where delivery belongs — the handlers stay unchanged:

| Call site | Template | Purpose |
| --- | --- | --- |
| `PublicSignUpAuthFactory` (email-verification OTP creation) | `EmailVerificationOtp` | `EnumOtpPurpose.EmailVerification` |
| `AdminForgotPasswordOtpFactory.CreatePasswordResetOtpAsync` | `PasswordResetOtp` | `EnumOtpPurpose.PasswordReset` |
| `PublicForgotPasswordOtpFactory` (same method, public side) | `PasswordResetOtp` | `EnumOtpPurpose.PasswordReset` |
| `AdminResendOtpFactory` / `PublicResendOtpFactory` | template matching the OTP's purpose | the two live purposes |
| `PublicVerifyOtp` success for an `EmailVerification` OTP | `Welcome` | — |
| `Public/AdminLogin` success | `LoginAlert` | — |

`EnumOtpPurpose.TwoFactorAuthentication` and `AccountRecovery` have **no
creation path in the codebase** — nothing ever writes an OTP with those
purposes, so there is nothing to deliver. They stay out of this spec until a
real flow creates them (see the note in spec 04).

Implementation shape inside a factory:

```csharp
await mailer.EnqueueAsync(
    template: EnumEmailTemplate.PasswordResetOtp,
    to: new EmailRecipient(user.Email, user.UserName),
    tokens: new Dictionary<string, string>
    {
        ["userName"] = user.UserName,
        ["otpCode"] = otp.Code,
        ["expiryMinutes"] = OtpConstants.ExpiryMinutes.ToString(),
    },
    culture: CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
    cancellationToken: cancellationToken
);
```

The enqueue joins the same `IUnitOfWork` commit that persists the OTP — if OTP
creation rolls back, no email goes out; if it commits, delivery is guaranteed
by the outbox (spec 05).

## Login alert scope

- Send only when the login introduces a **new device/IP combination** for the
  user, derived from the session metadata the login flow already captures for
  `SessionEntity` — not on every login (that trains users to ignore the
  alert).
- "New" = no prior session row for the user with the same IP + user-agent
  summary. This is a repository query, not new state; if the existing session
  data cannot answer it cheaply, ship v1 **without** the login alert rather
  than sending on every login — note the decision in this file.

## Enumeration-safety invariant

`AdminForgotPasswordHandler` (and the public variant) deliberately return
success for unknown emails. Nothing changes: the factory is only reached for
existing users, so no email means no probe signal. Verify in integration tests
that an unknown-email forgot-password request enqueues **zero** outbox rows
while still returning 200 (spec 09).

## Failure isolation

Auth flows must never fail because of email machinery: `EnqueueAsync` is a
local DB write, so the only way it fails is the same transaction failing —
acceptable and correct. No try/catch-and-swallow around enqueue: swallowing
would silently disable delivery, which is this spec's whole reason to exist.

## Checklist

- [x] Both live OTP purposes deliver through their factories
- [x] Resend flows re-deliver with the purpose-matching template
- [x] Welcome email on first successful email verification
- [x] Login alert **deferred**: the login flow exposes no cheap prior-session
      device/IP seam, and per this spec's own rule an every-login alert is
      worse than none. The `LoginAlert` template ships in the catalog
      (unit-covered) awaiting the session-metadata seam.
- [x] Enumeration-safe: unknown emails enqueue nothing, still 200
- [x] Handler/factory unit tests assert enqueue calls; endpoint integration
      tests assert outbox rows per spec 09

## Implementation notes

- Delivery hooks live where the data lives: the signup factory (has the OTP
  and user), the forgot-password handlers (capture the factory's returned
  OTP), the resend handlers (purpose-mapped template for the two live
  purposes; `TwoFactorAuthentication`/`AccountRecovery` rotate the row but
  send nothing — no template exists for flows that do not), and the public
  verify-otp handler (welcome on email verification only; the admin verify
  flow is not a signup and sends no welcome).
- Every hook skips silently when `user.Email` is null (social accounts).
