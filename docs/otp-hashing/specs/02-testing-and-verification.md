# Spec 02 — Testing and Verification

Read [01-hash-at-rest.md](01-hash-at-rest.md) first. The testing rulebook
([../../testing/00-unit-vs-integration-rules.md](../../testing/00-unit-vs-integration-rules.md))
binds as usual: unit tests prove the methods work, integration tests prove
the wired flows still deliver and verify codes end to end.

## Unit tests

- `OtpServiceTests`:
  - `GenerateOtpCode` returns exactly `OtpCodeLength` digits (format proof;
    the CSPRNG source is not unit-provable and does not need to be);
  - `CreateOtp` result: `Otp.CodeHash` starts with `v1:`, does **not** equal
    `PlainCode`, and `passwordService.Verify(PlainCode, Otp.CodeHash)` is
    true (use the real `PasswordService`, it has no dependencies);
  - expiry stamped from `OtpExpirationMinutes` as today.
- `OtpEntityTests`: `Create` stores the hash argument verbatim; the
  state-transition and guard tests are unaffected beyond the rename.
- Factory tests (signup, forgot ×2, resend ×2): adapt mocks to the
  `OtpCreationResult` return; assert the mailer token dictionary carries
  `PlainCode`, and that the **persisted** entity is `result.Otp` (the test
  that would have caught a plaintext leak into the row).
- Verify-flow handler tests: repository mock now returns the row for the
  identity lookup; assert the handler still marks the OTP used on success.
  The mismatch/attempt-increment logic lives in the repository and is
  integration territory.

## Integration tests

The existing email-delivery flow tests assert the outbox body contains the
exact code read from the OTP row — that direction is now impossible by
design, so those assertions **flip**:

- Extract the 6-digit code from the outbox row's text body with a regex
  (the only remaining place plaintext legitimately exists), then drive the
  real verify endpoint with it → success, account verified.
- Assert the `otps` row no longer contains the plaintext: `CodeHash` starts
  with `v1:` and does not equal the extracted code.
- Wrong code over real HTTP → `InvalidOtpCode` error and the attempt count
  increments; third wrong attempt → `MaxOtpAttemptsReached`.
- Full password-reset round trip (forgot → extract code → verify → reset)
  proves the used-OTP re-validation path against the hash.
- Resend invalidates the previous code: extract code A, resend, extract
  code B, verify with A fails, verify with B succeeds.

Modifying these pre-existing integration files is **correct and expected
here** — the persistence behavior they pinned is the thing this change
deliberately alters. That is the opposite situation from the domain-events
refactor's untouched-suites rule, where behavior was meant to be preserved;
record the distinction in the PR description rather than silently editing.

## Grep invariants (run before ticking anything)

- No reader of `otp.Code` / `Code =` on `OtpEntity` remains outside the
  migration history.
- `PlainCode` appears only in: the service that creates it, the factories
  that pass it to `EnqueueAsync` tokens, and tests. Never in a log call,
  an event payload, a DTO, or a persisted entity.
- `System.Random` no longer appears in `OtpService`.
- No specification type receives a plaintext code parameter.

## Deploy note (runbook)

The migration deletes all existing OTP rows: any code emailed before the
deploy dies with it. Users in mid-flow recover through the resend endpoint
(signup verification) or by re-requesting the reset (forgot password). Ship
it like any other migration — no coordination needed beyond knowing support
may see a brief "my code does not work" blip right after rollout.

## Checklist

- [x] Unit suite green, new `OtpService`/factory assertions included
- [x] Integration: verify/reset/resend round trips green via extracted codes
- [x] Integration: plaintext-absence assertion on the `otps` row
- [x] Grep invariants all clean
- [x] `dotnet build` clean; `dotnet csharpier .` no diff
- [x] Deploy note acknowledged in the PR description

## Implementation notes

### The OTP test fixtures hash the code they are given

`OtpBuilder` and `OtpFactory` still take a plaintext code, and now hash it with the production
`PasswordService` before handing it to `OtpEntity.Create`. Seeded OTPs therefore behave exactly
like ones the application issued: the same plaintext the test seeds is the plaintext the real
verify path accepts. That is what kept the existing verify, reset and resend endpoint tests
meaningful without rewriting them — they seed a known code and post that code, and the change
under test is invisible to them, which is the right outcome for a fix that must not alter the API.

### Pre-existing files modified, and why

| File | Change |
| --- | --- |
| `tests/Integration/Workflows/EmailDeliveryFlowTests.cs` | The signup assertion read the code out of the `otps` row and looked for it in the outbox body. That direction is now impossible by design, so it is inverted: the code is extracted from the outbox body and driven through the real verify endpoint, and the row is asserted to hold a `v1:` hash that is not that code. Three round-trip tests were added alongside it. |
| `tests/Integration/Modules/Identity/Infrastructure/Repositories/OtpRepositoryTests.cs` | `AddAsync_ShouldStoreOtpCodeCorrectly` asserted `saved.Code == ValidCode` — the exact behaviour this change removes. It now asserts the stored hash, and a wrong-code attempt-consumption test was added. |
| `tests/Unit/.../OtpServiceTests.cs`, `OtpEntityTests.cs`, `OtpRepositoryTests.cs`, `OtpSpecificationsTests.cs`, the five factory/handler test classes, `MockOtpService`, `MockOtpRepository`, `OtpBuilder`, `OtpFactory` | Adapted to the renamed property, the `OtpCreationResult` return, and the injected `IPasswordService`. |
| `tests/Unit/.../EntityConfigurationsTests.cs` | `OtpConfiguration_CodeProperty_ShouldBeRequired` pinned a column that no longer exists; it now pins `CodeHash`, its width, and the absence of the old one. |

Two unit tests changed meaning rather than shape, both following the repository inversion recorded
in spec 01: the expired-row case now reports expiry regardless of whether the submitted code
matched, and the old "no matching OTP, fall back to the latest valid one" test is now simply
"the code did not match the stored hash".

### Deploy note

Acknowledged. The migration clears `identity.otps`, so any code emailed before the deploy stops
working; users in mid-flow recover through resend or by re-requesting the reset.
