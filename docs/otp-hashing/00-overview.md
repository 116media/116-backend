# OTP Hashing at Rest

## The problem

One-time passwords are temporary authentication credentials, and they are the
only credential this codebase stores in cleartext. Passwords are PBKDF2-hashed
(`PasswordService`), refresh tokens are SHA-256-hashed before they touch the
sessions table (`RefreshTokenService`) — but `otps.code` holds the exact
6-digit string the user types. Anyone with read access to the database (a
dump, a backup, a compromised read replica, an over-privileged tool) holds
live login codes: a password-reset OTP read from that table is a full account
takeover, inside its 60-minute validity window, for any user with a pending
reset.

The fix is the same posture the other credentials already follow: store only
a hash, verify by comparison, and let the plaintext exist exclusively in
memory on its way into the email.

## Current state (measured)

| Concern | Where | Today |
| --- | --- | --- |
| Storage | `OtpEntity.Code`, `MaxLength(UserConstants.OtpCodeLength)` (6) | plaintext, straight from the generator into the column |
| Generation | `OtpService.GenerateOtpCode` | `System.Random` — **not cryptographically secure**; a seeded PRNG over a 10^6 keyspace is predictable |
| Creation sites | `PublicSignUpAuthFactory`, `PublicForgotPasswordOtpFactory`, `AdminForgotPasswordOtpFactory`, `PublicResendOtpFactory`, `AdminResendOtpFactory` | each calls `otpService.CreateOtp(...)`, commits, then enqueues the email with `otp.Code` |
| Verification lookup | `OtpRepository.ValidateOtpAsync` via `OtpForValidationSpecification(userId, code, purpose)` | the SQL query matches on the **plaintext code** |
| Used-OTP re-check | `OtpRepository.ValidateUsedOtpAsync` via `OtpForUsedValidationSpecification` | same by-code query shape (password-reset flow re-validates the consumed OTP) |
| Resend | `ResendOtpAsync` in both resend factories | already invalidates existing OTPs and issues a **fresh** code — no redesign needed here |
| Attempts / expiry | `UserConstants`: 60-minute expiry, 3 attempts max | enforced in `ValidateOtpAsync`, unchanged by this work |

Two findings, one wave:

1. **Plaintext at rest** — the headline fix.
2. **Non-cryptographic RNG** — `new Random()` generating credentials; replaced
   with `RandomNumberGenerator` in the same pass since it is two lines in the
   same service.

## Decisions

| Decision | Choice | Rationale |
| --- | --- | --- |
| Hash algorithm | Reuse `IPasswordService` (PBKDF2-SHA256, 25k iterations, salted, `v1:` format, constant-time verify) | A 6-digit code has a 10^6 keyspace: unkeyed SHA-256 (the refresh-token approach) is reversible with a phone-sized lookup table, so it is **not** acceptable here. PBKDF2's per-hash salt and work factor make bulk reversal of a dumped table expensive, the verify path is already constant-time, and no new secret has to be provisioned. Cost (~tens of ms) is irrelevant at OTP frequencies and attempts are capped at 3. |
| Rejected alternative | HMAC-SHA256 with a server-side pepper | Deterministic, so the by-code SQL lookup could survive — but it adds a new secret to manage across environments, and a leaked pepper collapses the whole table at once. Recorded in case hashing cost ever matters. |
| Lookup redesign | Query by `(userId, purpose, latest valid)`, verify the hash in memory | A salted hash cannot be queried by value. The creation paths already invalidate predecessors, so at most one active OTP exists per `(user, purpose)` — load it, verify against it. Details in [specs/01-hash-at-rest.md](specs/01-hash-at-rest.md). |
| Plaintext handling | `CreateOtp` returns the entity **and** the plain code as a pair; the plain code goes to the mailer tokens and nowhere else | The plaintext must never be reachable from a persisted row, an event payload, or a log line. |
| Existing rows | The migration deletes all `otps` rows before renaming the column | OTPs live 60 minutes; hashing old plaintext in a data migration is wasted effort for rows that are dead by the next deploy window. In-flight codes at deploy time die — users hit resend. |
| API contract | Unchanged | Request/response shapes, status codes, and error titles stay identical; frontend, dashboard, and mobile need nothing. |

## Explicitly out of scope (recorded, not forgotten)

- **Outbox email bodies** — `mailer.outbox_emails` retains rendered bodies,
  which contain the plaintext codes it delivered. Hashing the `otps` table
  does not scrub those. The fix belongs to the outbox retention/purge
  decision already tracked in the email-service docs, not here.
- **`CleanupExpiredOtpsAsync` dead code** — tracked in the domain-events
  spec 04 notes; hashing makes stale rows less sensitive but the purge is
  still worth wiring in its own change.
- **Rate limiting / attempt caps** — already in place and untouched.

## Documents

| # | File | Covers |
| --- | --- | --- |
| 01 | [specs/01-hash-at-rest.md](specs/01-hash-at-rest.md) | Entity, service, repository, specification, and migration changes; the five call sites |
| 02 | [specs/02-testing-and-verification.md](specs/02-testing-and-verification.md) | Test changes at both layers, grep invariants, deploy note, checklist |
