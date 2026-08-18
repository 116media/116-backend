# Stage 5 — Password & OTP security

Closes **[07 S4]** (the password-reset OTP is never consumed, so one observed code resets the
password repeatedly for an hour), **[07 S5]** (OTP attempt limiting is per-row, so `resend-otp`
resets the counter — ~3 guesses per resend, forever, with no lockout), **[07 S7]** (login,
forgot-password and resend-OTP disclose whether an account exists and whether it is an admin), and
**[07 S10]** (PBKDF2-SHA256 at 25,000 iterations, a 6-character minimum, and no lockout anywhere).

It also closes the security half of **[07 A5]** — `verify-otp` marks the account email-verified for
*every* purpose — and a two-part flaw the audit did not name, which together make `reset-password`
an unmetered code-guessing oracle:

- `InvalidateExistingOtpsAsync` selects unused codes (`OtpForInvalidationSpecification` = user ∧
  purpose ∧ **not used**) and marks them `IsUsed = true`. That is the exact predicate
  `ValidateUsedOtpAsync` searches for (`OtpForUsedValidationSpecification` = user ∧ purpose ∧
  **used**), so a code that was merely superseded by a resend — never verified by anyone — becomes
  a valid password-reset credential.
- `ValidateUsedOtpAsync` performs **no attempt counting**. Unlike `ValidateOtpAsync`, which
  increments `AttemptCount` and enforces `MaxOtpAttempts`, the reset path simply throws
  `OtpNotYetVerified` on a mismatch and leaves no trace.

Chained: `resend-otp` is anonymous, so anyone can force a victim's outstanding code into the `used`
state, and then guess 6-digit codes at `reset-password` with no attempt limit, no lockout, and no
alert — bounded only by the rate limiter. A hit resets the password and revokes every session.

> **Breaking change.** OTP rows are
> re-hashed with a new keyed scheme, so every OTP outstanding at deploy is invalidated. A new
> `OTP_PEPPER` secret is required. See [Rollout](#rollout).

> **Depends on Stage 4** (branch stacks on the tree Stage 4 landed on).

---

## Design — make the slow thing slow and the cheap thing cheap

The current design has one hashing primitive doing two unrelated jobs. `OtpService.CreateOtp` calls
`IPasswordService.Hash`, and `OtpRepository` calls `IPasswordService.Verify` twice, so every OTP
create and check pays a full PBKDF2 stretch. Raising the password work factor to 600,000 without
separating them would multiply the cost of every OTP operation by 24.

So the two are split by what they defend against:

| | Password | OTP |
| --- | --- | --- |
| Threat | offline cracking of a leaked hash table | online guessing of a live code |
| Primitive | PBKDF2-SHA256, 600,000 iterations, per-hash salt | HMAC-SHA256 with a server-side pepper |
| Why | cost per guess is the only defence | a keyed hash makes a DB-only leak useless; cost per guess is controlled by lockout, not by CPU |

A 6-digit code has 10⁶ possibilities, which no hash cost can defend. The control that actually works
is **per-account lockout**, which is why S5 and S10 share the same fix: counters that live on the
account rather than on the OTP row or nowhere at all. Those counters are incremented with atomic
`ExecuteUpdateAsync` statements — the same pattern Stage 4 used for token-version bumps — because
the increment must survive the exception that the failed attempt throws.

Enumeration is closed by making the unknown-account path do the same work and return the same
answer as the known-account path, rather than by hiding the status code alone.

---

## Open decisions

These change the shape of the work. The spec below implements the **recommended** column; say the
word and I will rewrite the affected sections.

| # | Decision | Options | Recommended |
| --- | --- | --- | --- |
| D1 | OTP code length | keep **6** digits, or raise to **8** as the audit suggests | **Keep 6.** Lockout, not entropy, is what stops online guessing, and 8 digits touches the validator, the regex, every fixture and the two email templates for a control that lockout already provides. |
| D2 | Scope of [07 A5] | **minimal** (only `EmailVerification` marks the account verified), or the audit's **full split** into `VerifyEmailOtp` + `ValidatePasswordResetOtp` returning a single-use ticket | **Minimal.** It removes the vulnerability without a public API break; the full split is a use-case redesign that belongs with the Identity restructure. |
| D3 | Lockout counters | **two pairs** (login and OTP tracked separately), or **one shared** lockout | **Two pairs.** A shared counter lets OTP guessing lock the victim out of login, which widens the denial-of-service the lockout itself introduces. |
| D4 | Password minimum | **12** characters per the audit, or leave at 6 | **Left at 6** by decision after review; see Part A. The audit's case for 12 still stands and is not closed by anything else in this stage. |
| D5 | Symbol requirement | add a symbol class to the complexity regex, or keep lower/upper/digit | **Keep.** Length dominates; adding a class now invalidates fixtures for little gain. |

---

## Checklist

- [x] 5.1 — `PasswordService`: `v2:` at 600,000 iterations, `Verify` still accepts `v1:`, add `NeedsRehash`
- [x] 5.2 — Lazy re-hash on successful login (public + admin)
- [ ] 5.3 — `MinPasswordLength` left at 6 (deferred; see the note in Part A)
- [x] 5.4 — `IOtpService` hashes with HMAC-SHA256 + `OTP_PEPPER`; `OtpService`/`OtpRepository` stop using `IPasswordService`
- [x] 5.5 — `OtpEntity.ConsumedAt` + `MarkAsConsumed`; reset consumes; resend invalidates via consumption, not `MarkAsUsed`; `ValidateUsedOtpAsync` counts failed attempts
- [x] 5.6 — `OtpExpirationMinutes` left at 60; consumption, not expiry, closes the replay
- [x] 5.7 — `verify-otp` marks the account verified only for `EmailVerification`
- [x] 5.8 — Per-account OTP lockout + resend cap
- [x] 5.9 — Per-account login lockout
- [x] 5.10 — Constant-time login; remove the 404 branch and its `.Produces`
- [x] 5.11 — Forgot-password and resend-OTP answer identically for unknown, inactive, unverified and non-admin
- [x] 5.12 — Migration `AddAccountLockoutAndOtpConsumption` (+ OTP table clear)
- [x] 5.13 — Unit + integration tests
- [ ] 5.14 — Verify (build 0/0, unit green; run integration locally)

---

## Part A — Password stretching `[07 S10]`

### 5.1 A second hash version

`PasswordService` currently hard-codes `Iterations = 25000` and rejects anything that does not start
with `v1:`. It gains a second version and keeps reading the first:

```csharp
private const int LegacyIterations = 25_000;
private const int CurrentIterations = 600_000;
private const string LegacyPrefix = "v1:";
private const string CurrentPrefix = "v2:";
```

`Hash` emits `v2:`. `Verify` picks the iteration count from the prefix and returns false for
anything else, so existing `v1:` hashes keep verifying at their original cost. A third member tells
callers a stored hash is behind:

```csharp
/// <summary>
/// Whether the stored hash was produced by an older work factor and should be replaced.
/// </summary>
/// <param name="hash">The stored hash.</param>
/// <returns>True when the hash is missing or not the current version.</returns>
bool NeedsRehash(string? hash);
```

`Verify` also gains the constant-work path §5.10 needs:

```csharp
/// <summary>
/// Verifies against the stored hash, or against a constant hash of equal cost when the account
/// has none, so an unknown account and a wrong password take the same time.
/// </summary>
/// <param name="password">The supplied password.</param>
/// <param name="hash">The stored hash, or null.</param>
/// <returns>True only when a real hash was supplied and matched.</returns>
bool VerifyOrDummy(string password, string? hash);
```

The dummy hash is a `static readonly Lazy<string>` produced once per process from a random secret,
so the padding costs one stretch at startup rather than one per request.

### 5.2 Lazy re-hash

`PublicLoginAuthFactory` and `AdminLoginAuthFactory` re-hash after a successful verify:

```csharp
if (passwordService.NeedsRehash(user.PasswordHash))
{
    user.InitializePasswordHash(passwordService.Hash(password), errors: userErrors);
}
```

`InitializePasswordHash` is the right method: it installs a hash without raising
`UserPasswordChangedEvent`, so a work-factor upgrade does not send the owner a "your password
changed" email. The mutation rides the commit `SessionFactory.CreateSessionAsync` already performs
later in the same request, so the login path gains no extra round trip.

### 5.3 Password minimum

`UserConstants.MinPasswordLength` **stays at 6**, against the audit's suggestion of 12. This is a
deliberate deferral, not a fix: unlike the OTP expiry, the raised work factor does **not** fully
compensate. A 6-character password drawn from the current complexity classes is a space an offline
attacker can exhaust in hours to days even at 600,000 iterations, whereas 12 characters puts it out
of reach. It is also below the NIST SP 800-63B floor of 8 for user-chosen secrets. Raising it
remains the single highest-value change still open on this file.

The length rule is enforced in exactly one place —
`CredentialValidation.ValidPassword` — so the six strong-password validators pick it up for free and
the two login validators (`isStrong: false`) are untouched, which matters: a legacy 8-character
password must still be able to log in.

---

## Part B — OTP integrity `[07 S4]` + `[07 A5]`

### 5.4 A dedicated OTP hashing scheme

`IOtpService` gains the two members that stop OTP codes sharing the password scheme:

```csharp
public interface IOtpService
{
    /// <summary>
    /// Hashes a plaintext OTP code for storage.
    /// </summary>
    /// <param name="code">The plaintext code.</param>
    /// <returns>The keyed hash to persist.</returns>
    string Hash(string code);

    /// <summary>
    /// Verifies a supplied code against a stored hash in constant time.
    /// </summary>
    /// <param name="code">The supplied code.</param>
    /// <param name="hash">The stored hash.</param>
    /// <returns>True when the code matches.</returns>
    bool Verify(string code, string? hash);
}
```

HMAC-SHA256 keyed with `OTP_PEPPER`, emitting `h1:{base64}` and comparing with
`CryptographicOperations.FixedTimeEquals`. The pepper is read through
`AppEnvironment.OtpPepper()` and the module refuses to start without one outside Development, the
same fail-closed posture Stage 3 gave CORS.

The keyed construction is the point: a 6-digit code behind an unkeyed hash is recoverable from a
database dump in microseconds, whereas the pepper lives only in the application's environment.

`OtpService` owns the scheme rather than delegating to a collaborator, matching how `PasswordService`
self-contains its own derivation; it takes the pepper through its constructor and is registered with
a factory that supplies `AppEnvironment.OtpPepper()`. `OtpRepository` depends on `IOtpService` at both
comparison sites, in place of the `IPasswordService` it used before. The existing
`OtpCodeHashLength = 100` already accommodates the shorter output, so no column change is needed.

### 5.5 Consumption, distinct from use

`IsUsed` currently means two different things — "the owner verified this code" and "a resend
superseded this code" — and `ValidateUsedOtpAsync` accepts both. `OtpEntity` gains a separate,
one-way terminal state:

```csharp
/// <summary>
/// UTC timestamp at which the code was spent or superseded; a consumed code is never valid again.
/// </summary>
public DateTime? ConsumedAt { get; private set; }

/// <summary>
/// Marks the code spent, so it cannot be presented again.
/// </summary>
public void MarkAsConsumed()
{
    ConsumedAt = DateTime.UtcNow;
}

/// <summary>
/// Whether the code has already been spent or superseded.
/// </summary>
/// <returns>True once consumed.</returns>
public bool IsConsumed()
{
    return ConsumedAt is not null;
}
```

A fourth change closes the oracle half of the flaw: `ValidateUsedOtpAsync` must meter its failures
the way `ValidateOtpAsync` already does — incrementing the row's `AttemptCount` and calling
`RegisterFailedOtpAsync` (§5.8) before throwing — so guessing at `reset-password` costs the same as
guessing at `verify-otp` and trips the same account lock.

Three further consequences:

- `OtpForValidationSpecification` (verify) and `OtpForUsedValidationSpecification` (reset) both gain
  `ConsumedAt == null`, via a new `OtpIsNotConsumedSpecification`.
- `InvalidateExistingOtpsAsync` calls `MarkAsConsumed()` instead of `MarkAsUsed()`, so a superseded
  code is no longer indistinguishable from a verified one.
- The reset handlers call `otp.MarkAsConsumed()` between `ValidateUsedOtpAsync` and
  `ResetPasswordAsync`. Both run on the same scoped context, so the existing `CommitAsync` inside
  `ResetPasswordAsync` persists the consumption and the new password in one transaction — the code
  cannot survive the reset it performed.

### 5.6 Expiry

`UserConstants.OtpExpirationMinutes` stays at **60**, against the audit's suggestion of 10.

The audit's finding was that one code resets the password *repeatedly* for the rest of its window.
That is closed by consumption (§5.5), not by shortening the window: a code is spent the moment it
drives a reset and the reset lookup ignores consumed rows. The brute-force argument is likewise
answered by the account counter (§5.8), which caps an attacker at `MaxAccountOtpAttempts` guesses no
matter how long the code lives.

What a shorter window would still buy is a narrower exposure period for a code that was intercepted
but never used — a forwarded mail, a shared mailbox, a gateway log. That is real but far smaller
than the replay loop, and it has to be weighed against two costs specific to this codebase: the
expiry gates **both** steps of the reset flow (`IsExpired` is checked in `ValidateOtpAsync` and
again in `ValidateUsedOtpAsync`), so the clock covers delivery, entry and password choice; and the
new resend cap (§5.8) means a user with slow mail can exhaust their resends chasing a code that
keeps dying.

A future refinement is to make the expiry purpose-aware — a short window for `PasswordReset`, a
generous one for `EmailVerification` — since `CalculateExpirationTime` already receives the purpose.
That is deliberately out of scope here.

### 5.7 Purpose-scoped verification

`PublicVerifyOtpHandler` and `AdminVerifyOtpHandler` call `user.MarkAsVerified()` unconditionally.
It becomes conditional:

```csharp
if (purpose.Value == EnumOtpPurpose.EmailVerification)
{
    user.MarkAsVerified();
}
```

Completing a password reset — or minting and verifying one of the two flowless purposes — no longer
sets `IsVerified = true` on an address nobody confirmed. Verification is the module's only
legitimacy gate, so this is a correctness fix as much as a security one.

---

## Part C — Lockout `[07 S5]` + `[07 S10]`

### 5.8 and 5.9 Per-account counters

`UserEntity` gains four columns and no behaviour — the counters are moved by atomic SQL, never by a
tracked mutation, so a failed attempt is recorded even though the request then throws:

| Column | Meaning |
| --- | --- |
| `FailedLoginAttempts` | consecutive wrong passwords |
| `LockedUntil` | login refused until this UTC instant |
| `OtpFailedAttempts` | consecutive wrong OTP codes, across resends |
| `OtpLockedUntil` | OTP verification refused until this UTC instant |

New `IAccountLockoutRepository` (Application) + implementation (Infrastructure), deliberately kept
off the already-overloaded `IAuthRepository`:

```csharp
Task<AccountLockoutState> GetAsync(Guid userId, CancellationToken cancellationToken);
Task<int> RegisterFailedLoginAsync(Guid userId, CancellationToken cancellationToken);
Task ClearFailedLoginsAsync(Guid userId, CancellationToken cancellationToken);
Task LockLoginUntilAsync(Guid userId, DateTime until, CancellationToken cancellationToken);
Task<int> RegisterFailedOtpAsync(Guid userId, CancellationToken cancellationToken);
Task ClearFailedOtpAsync(Guid userId, CancellationToken cancellationToken);
Task LockOtpUntilAsync(Guid userId, DateTime until, CancellationToken cancellationToken);
```

Each `Register*` is a single `ExecuteUpdateAsync` increment returning the new count, so two
concurrent guesses cannot both read 2 and write 3.

New constants in `UserConstants`: `MaxLoginAttempts = 5`, `LoginLockoutMinutes = 15`,
`MaxAccountOtpAttempts = 5`, `OtpLockoutMinutes = 15`, `MaxOtpResendsPerWindow = 3`,
`OtpResendWindowMinutes = 15`. `MaxOtpAttempts = 3` stays as the per-row cap; the account cap sits
above it and is the one a resend cannot reset.

Wiring:

- Login factories check `LockedUntil` before verifying, call `RegisterFailedLoginAsync` on a wrong
  password (locking at the threshold), and `ClearFailedLoginsAsync` on success.
- `OtpRepository.ValidateOtpAsync` calls `RegisterFailedOtpAsync` alongside the existing per-row
  increment, and the verify handlers call `ClearFailedOtpAsync` on success.
- The resend factories count rows created for the account and purpose inside
  `OtpResendWindowMinutes` and stop issuing beyond `MaxOtpResendsPerWindow`.

> **Lockout is a denial-of-service primitive.** Anyone who knows an address can lock its login for
> 15 minutes. That is the accepted trade — the window is short, it is per-account rather than
> global, and Stage 3 already partitions the rate limiter per caller so the lock is not the only
> control. The alternative, escalating delays, keeps the account reachable but ties up a request
> thread per guess, which is worse under the same attack.

---

## Part D — Enumeration `[07 S7]`

### 5.10 Constant-time login

Both login factories look the account up through a method that throws `NotFoundException` before the
password is ever verified, which is both a status oracle (404 versus 401) and a timing oracle (no
PBKDF2 work on the unknown branch). `IAuthRepository` gains non-throwing siblings —
`GetUserWithRolesAndPermissionsByCredentialsAsync` and `...ByEmailAsync` — and the factories become:

```csharp
UserEntity? user = await authRepository.GetUserWithRolesAndPermissionsByCredentialsAsync(
    credentials: credentials,
    cancellationToken: cancellationToken
);

// Runs the full stretch even when the account is unknown, so both branches cost the same.
bool passwordMatches = passwordService.VerifyOrDummy(password: password, hash: user?.PasswordHash);

if (user is null || !passwordMatches)
{
    throw userErrors.InvalidCredentials();
}
```

Unknown account and wrong password now both produce `AuthenticationException` → 401 after one full
stretch. `.Produces(StatusCodes.Status404NotFound)` comes off both login endpoints because the
branch no longer exists. Account status is still reported after a correct password, which is not an
oracle: the caller already proved they hold the credential.

### 5.11 One answer for forgot-password and resend

`AdminForgotPasswordHandler` answers 200 for an unknown address, **403 for a known non-admin**, and
200 for an admin, which identifies every privileged account. The public handler leaks the same way
through `AccountInactiveException` and `AccountNotVerifiedException`.

Both handlers, public and admin, collapse to the shape the unknown-address branch already returns:
look the account up, and when it is missing, inactive, unverified, not an admin, or over its resend
cap, return the same success result without sending mail — and log the real reason at
`Information` server-side. The same applies to the resend handlers, whose `IsUserAdmin` and
`IsUserAccountActive` calls leak identically.

`verify-otp` keeps its 404, because reaching it requires already holding a code for the address.

### 5.12 Migration

`AddAccountLockoutAndOtpConsumption` adds `failed_login_attempts` (int, default 0), `locked_until`
(timestamptz, null), `otp_failed_attempts` (int, default 0), `otp_locked_until` (timestamptz, null)
to `users`, and `consumed_at` (timestamptz, null) to `otps`. It then clears `otps` outright:
existing rows are hashed with PBKDF2 and cannot be verified by the new keyed hasher, and every one
of them expires within the hour anyway.

```sql
DELETE FROM identity.otps;
```

Leave unapplied, per convention.

---

## Tests

The behaviour changes here invalidate a large number of existing assertions. That is expected and
each one is a real contract change, not a test to loosen.

- **Unit**
  - `PasswordService`: `Hash` emits `v2:`; `Verify` accepts both `v1:` and `v2:`; `NeedsRehash` is
    true for `v1:`/null and false for `v2:`; `VerifyOrDummy` returns false for a null hash and
    performs a real stretch. The existing tests asserting the literal `v1:` prefix move to `v2:`.
  - `OtpService` hashing: same code hashes equal, different codes differ, a different pepper fails
    to verify, a tampered or unparsable hash fails, and construction without a pepper throws.
  - `OtpEntity`: `MarkAsConsumed` sets `ConsumedAt`; `IsConsumed` flips; consumption is independent
    of `IsUsed`.
  - `OtpRepository`: a consumed row is invisible to both validators; `InvalidateExistingOtpsAsync`
    consumes rather than marks used.
  - Login factories: unknown account throws `AuthenticationException` (not `NotFoundException`) and
    still calls verify; a `v1:` hash is re-hashed on success; lockout refuses before verifying.
  - Verify handlers: `MarkAsVerified` is called for `EmailVerification` and **not** for
    `PasswordReset`. This is new coverage rather than an inversion — no existing verify-otp test,
    unit or integration, uses any purpose other than `EmailVerification`, which is exactly why the
    bug survived.
  - Forgot/resend handlers: non-admin, inactive and unverified all return the success shape.
  - Validators: the length rule is unchanged at 6, so no fixture password needed lengthening.
- **Integration** (real HTTP)
  - Login with an unknown email returns **401**, not 404 — inverting the three `NotFoundException`
    assertions in `PublicLoginEndpointV1Tests` (`:58`, `:340`, `:370`, the last two being the
    localized variants) and `AdminLoginEndpointV1Tests:60`.
  - Six wrong passwords lock the account; the seventh is refused with the correct password.
  - A password reset consumes its code: replaying the same code returns 400.
  - `resend-otp` no longer resets the account counter — resend, then guess, and the account still
    locks.
  - Admin forgot-password returns the same 200 for an unknown address and a known non-admin,
    inverting `AdminForgotPasswordEndpointV1Tests.ForgotPassword_ForNonAdminUser_ReturnsForbidden`.
  - Reset-password is metered: repeated wrong codes trip the account OTP lock, and a code that was
    only superseded by a resend is refused outright.
  - Verifying a `PasswordReset` code leaves `IsVerified` false.
  - A legacy `v1:` hash seeded directly is upgraded to `v2:` after one successful login.

---

## Rollout

1. Provision `OTP_PEPPER` (32+ random bytes, base64) in every environment before deploy; the
   Identity module refuses to start without it outside Development.
2. Ship the migration; it clears outstanding OTPs, so anyone mid-verification must request a new
   code. Communicate or deploy in a quiet window.
3. Expect login latency to rise by roughly the cost of one 600,000-iteration stretch. Change-password
   verifies twice (old password, then new-equals-old), so that endpoint pays it twice.
4. Existing `v1:` hashes upgrade silently on each owner's next login; no bulk migration is possible
   because the plaintext is not recoverable.

## Verification

1. `dotnet build 116_backend.sln` — 0 warnings / 0 errors.
2. `dotnet test tests/Unit` — green.
3. Run `tests/Integration` locally.
4. Confirm the migration adds only the five columns and the OTP clear, and nothing else.

**PR title:** `fix(auth): strengthen password hashing, OTP consumption and account enumeration`
