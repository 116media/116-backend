# Spec 01 — Hash the OTP at Rest

Read [../00-overview.md](../00-overview.md) first for the threat model and
the recorded decisions. This spec is the implementation order.

## 1. Entity — `OtpEntity`

- Rename `Code` to `CodeHash`. The property now documents that it holds the
  PBKDF2 output, never the deliverable code.
- Replace `[MaxLength(UserConstants.OtpCodeLength)]` with a length that fits
  the `v1:{base64(salt16 + hash32)}` format (67 characters today) — add
  `UserConstants.OtpCodeHashLength = 100` and use it, leaving headroom for a
  future `v2:` format the same way `users.password_hash` absorbs one.
- `Create(...)` takes `codeHash` instead of `code`. No other member changes:
  `MarkAsUsed`, `IncrementAttemptCount`, `IsValid`, `IsExpired`,
  `HasMaxAttemptsReached` are all code-agnostic already.

## 2. Generation — `OtpService`

Two changes in `Infrastructure/Services/OtpService.cs`:

- `GenerateOtpCode` switches from the shared `System.Random` instance to
  `RandomNumberGenerator.GetInt32(0, 1_000_000)` formatted with
  `$"D{UserConstants.OtpCodeLength}"`. Same output shape, cryptographically
  secure source, and the service loses its mutable field.
- `CreateOtp` hashes before building the entity and must hand the plaintext
  back to the caller, because the entity no longer carries it:

```csharp
public OtpCreationResult CreateOtp(Guid userId, EnumOtpPurpose purpose)
{
    string plainCode = GenerateOtpCode();
    string codeHash = passwordService.Hash(password: plainCode);
    DateTime expiresAt = CalculateExpirationTime();

    OtpEntity otp = OtpEntity.Create(
        Guid.NewGuid(),
        userId: userId,
        codeHash: codeHash,
        purpose: purpose,
        expiresAt: expiresAt
    );

    return new OtpCreationResult(Otp: otp, PlainCode: plainCode);
}
```

- `OtpCreationResult(OtpEntity Otp, string PlainCode)` is a record co-located
  with `IOtpService` (the ImageColors precedent: results live next to the
  port that returns them, no `Models/` folder).
- `OtpService` gains an `IPasswordService` constructor dependency — already
  registered in `IdentityModule`, no DI changes beyond the constructor.
- `PlainCode` is in-memory only: it may be passed to `IMailer.EnqueueAsync`
  tokens and nothing else — never persisted, never logged, never placed on
  an event payload.

## 3. The five creation call sites

Each site changes mechanically from entity-carries-code to result-pair:

| Site | Change |
| --- | --- |
| `PublicSignUpAuthFactory` | `var otpResult = otpService.CreateOtp(...)`; persist `otpResult.Otp`; email token `["otpCode"] = otpResult.PlainCode` |
| `PublicForgotPasswordOtpFactory` / `AdminForgotPasswordOtpFactory` | same substitution |
| `PublicResendOtpFactory` / `AdminResendOtpFactory` | same substitution inside `ResendOtpAsync`; the invalidate-then-create shape is untouched |

Handlers that receive the created OTP for the email enqueue take the result
record (or just the `PlainCode` string) instead of reading `otp.Code`.

## 4. Verification — `OtpRepository`

A salted hash cannot be matched in SQL, so the by-code specifications die and
the lookup inverts: **load the candidate row by identity, verify the code in
memory.**

The standing invariant that makes this sound: every creation path invalidates
the user's existing OTPs for that purpose first (resend and forgot do so
explicitly; signup is the account's first OTP), so at most one valid row per
`(userId, purpose)` exists at any time.

### `ValidateOtpAsync(userId, code, purpose)`

Preserve the exact public error semantics, in this order:

1. Load the latest valid-shaped OTP for `(userId, purpose)` (the existing
   `GetLatestValidOtpAsync` query shape — not filtered by code).
2. No row → `userErrors.NoValidOtpFound()`.
3. `IsExpired()` → `userErrors.OtpExpired()`.
4. `HasMaxAttemptsReached()` → `userErrors.MaxOtpAttemptsReached()`.
5. `passwordService.Verify(code, otp.CodeHash)`:
   - match → return the entity (handler marks it used, as today);
   - mismatch → `IncrementAttemptCount()`, save, then
     `MaxOtpAttemptsReached()` if the increment consumed the last attempt,
     else `userErrors.InvalidOtpCode()`.

This collapses today's two-branch flow (exact-match query plus
latest-valid fallback) into one path with identical observable behavior —
the fallback branch existed only because the primary query filtered by code.

### `ValidateUsedOtpAsync(userId, code, purpose)`

Same inversion against the used-row query shape: load the latest **used**
OTP for `(userId, purpose)`, verify the hash in memory, keep the existing
error semantics. This is the password-reset flow re-validating the consumed
OTP, so the hash comparison must succeed against a row where `IsUsed` is
true.

### Specifications

- `OtpForValidationSpecification` and `OtpForUsedValidationSpecification`
  lose their `code` parameter (or are replaced by the by-user-and-purpose
  variants if those already exist) — a specification can never see a
  plaintext code again.
- Specifications stay covered through the repository methods that use them,
  named in the doc comment, per the testing rulebook.

## 5. Migration

One migration on `IdentityDbContext` (module's existing
`Infrastructure/Persistence/Migrations` output dir):

- `DELETE FROM authentication.otps` — rows are ephemeral (60-minute
  lifetime); hashing them retroactively is pointless and the plaintext must
  not survive the rename.
- Rename `code` to `code_hash`, widen to `OtpCodeHashLength`.

Generated with the standard command (`--context IdentityDbContext`), left
unapplied like every prior migration.

## 6. What must NOT change

- Request/response contracts, status codes, and error titles on signup,
  verify-otp, forgot-password, reset-password, and resend-otp — all five
  flows behave identically from the client's seat.
- The OTP email itself: template, tokens, and the direct post-commit
  `EnqueueAsync` call (the documented domain-events exclusion). This change
  is the reason that exclusion exists — after it lands, the plaintext
  genuinely exists only inside the creating flow.
- Expiry, attempt caps, invalidation cascades, rate limits.

## Checklist

- [ ] `CodeHash` entity rename + `OtpCodeHashLength` constant
- [ ] CSPRNG generation
- [ ] `CreateOtp` returns `OtpCreationResult`; `IPasswordService` injected
- [ ] Five call sites updated; no reader of `otp.Code` remains anywhere
- [ ] Repository verification inverted; by-code specifications gone
- [ ] Migration (delete + rename + widen) generated
- [ ] Error semantics verified unchanged against the table in section 4
