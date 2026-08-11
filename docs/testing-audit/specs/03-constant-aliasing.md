# Spec 03 — Constant aliasing

## Goal

Nine `TestConstants` partials declare in their own doc comments that they mirror a
production constants file. They are literal copies with no compile-time link, and seven
values have drifted. One of them — `Otp.MaxAttempts` at 5 against a production threshold
of 3 — makes the only test of the OTP brute-force lockout pass for every possible
threshold value, so a security control is currently untested. This spec replaces the
copies with `const` aliases of the production constants, which is compile-time
substitution with no runtime cost, and fixes the two OTP artefacts the drift produced.
It is early because it is cheap, because the aliasing turns a class of silent blind spot
into a build-time link, and because every later spec that touches a boundary assertion
should be reading a number that is true.

Backing finding: [../fixtures/01-constant-drift.md](../fixtures/01-constant-drift.md).

## Scope

In this spec:

- The nine `TestConstants` partials under `tests/Fixtures/Constants/` alias their
  production counterparts wherever one exists, and document the values that are
  genuinely test-owned.
- `OtpEntityTests` asserts both sides of the lockout boundary.
- `OtpBuilder.AsMaxAttemptsReached()` produces a state production can actually reach.
- Any test that turns red because it was asserting past the real edge is fixed as a
  test.

Not in this spec:

- Changing any production constant. If a production limit looks wrong, that is a
  separate conversation with a separate ticket; nothing here edits `src/`.
- The `UserBuilder` uniqueness rework, which reads
  `TestConstants.User.UserNameMaxLength` — [02-test-isolation.md](02-test-isolation.md)
  Change 5. This spec makes that constant correct; the builder edit belongs there.
- `TestConstants.ApiRoutes` and `TestConstants.ValidationMessages`. The first already
  composes from production route constants and needs nothing; the second holds English
  message fragments that belong to spec 06's localization work.

## Prerequisites

None. This spec can land before, after, or in parallel with 01 and 02.

Land it before [02-test-isolation.md](02-test-isolation.md) Change 5 if possible, so the
`UserBuilder` username truncation is written against 20 rather than 50.

## Changes

### 1. Alias the production constants in the nine partials

Files: the nine files under `tests/Fixtures/Constants/`. The project already references
`BuildingBlocks`, `Identity`, `Core` and `Content`
(`tests/Fixtures/_116.Tests.Fixtures.csproj`), so every alias below compiles with a
`using` and no new dependency.

`const int X = UserConstants.Y;` is a compile-time constant expression. The emitted IL is
identical to the literal it replaces, so there is no runtime cost, and a change in `src/`
reaches every boundary assertion on the next build.

The mapping, file by file:

| Test partial | Production source | Aliases |
| --- | --- | --- |
| `Identity/TestConstants.Otp.cs` | `UserConstants` | `CodeLength`, `MaxAttempts`, `ExpirationMinutes` |
| `Identity/TestConstants.User.cs` | `UserConstants` | `EmailMaxLength`, `UserNameMaxLength`, `UserNameMinLength`, `PasswordMinLength`, `CountryMaxLength`, `PhoneMaxLength` |
| `Identity/TestConstants.Session.cs` | `SessionConstants` | `DeviceIdMaxLength`, `IpAddressMaxLength`, `UserAgentMaxLength` |
| `Identity/TestConstants.Role.cs` | `RoleConstants` | `NameMaxLength`, `DescriptionMaxLength` |
| `Identity/TestConstants.Permission.cs` | `PermissionConstants` | `ResourceMaxLength`, `ActionMaxLength`, `DescriptionMaxLength` |
| `Core/TestConstants.File.cs` | `FileConstants` | `FileNameMaxLength`, `MimeTypeMaxLength`, `StorageUrlMaxLength` |
| `Content/TestConstants.Content.cs` | `ContentConstants` | every `*MaxLength` and `*MinLength` under `ContentType`, `PricingTier`, `PromotionLevel`, `Tag`, `Category`, `Customer`, `Package`, `Editorial.*` and `Interactions` |
| `Identity/TestConstants.Auth.cs` | none | no numeric constants; doc comment only |
| `Identity/TestConstants.Jwt.cs` | none | test-owned values; doc comment only |

The OTP partial is the one that matters most:

```csharp
// tests/Fixtures/Constants/Identity/TestConstants.Otp.cs — before
public static class Otp
{
    public const int CodeLength = 6;
    public const int MaxAttempts = 5;
    public const int ExpirationMinutes = 10;

    public const string ValidCode = "123456";
    public const string InvalidCode = "000000";
    public const string DefaultCode = "654321";
}
```

```csharp
// after
using _116.BuildingBlocks.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for OTP entity testing. The numeric limits alias
    /// <see cref="UserConstants" /> rather than copying it, so a change to a production
    /// limit reaches every boundary assertion at compile time.
    /// </summary>
    public static class Otp
    {
        /// <summary>
        /// The production OTP code length.
        /// </summary>
        public const int CodeLength = UserConstants.OtpCodeLength;

        /// <summary>
        /// The production brute-force lockout threshold. An OTP is locked once
        /// <c>AttemptCount</c> reaches this value, so it is also the highest count a
        /// persisted row can hold.
        /// </summary>
        public const int MaxAttempts = UserConstants.MaxOtpAttempts;

        /// <summary>
        /// The production OTP validity window in minutes.
        /// </summary>
        public const int ExpirationMinutes = UserConstants.OtpExpirationMinutes;

        /// <summary>
        /// A well-formed code used wherever a test needs a code that parses.
        /// Test-owned: production generates codes rather than declaring them.
        /// </summary>
        public const string ValidCode = "123456";

        /// <summary>
        /// A well-formed code that is never the one under test.
        /// </summary>
        public const string InvalidCode = "000000";

        /// <summary>
        /// The code <c>OtpBuilder</c> uses when a test does not specify one.
        /// </summary>
        public const string DefaultCode = "654321";
    }
}
```

`TestConstants.User.cs` takes the same treatment. Note that `PasswordMaxLength = 128` has
**no** production counterpart — `UserConstants` declares `MinPasswordLength` and nothing
above it — so it stays a literal and says so:

```csharp
// tests/Fixtures/Constants/Identity/TestConstants.User.cs — after (numeric section)
/// <summary>
/// The production maximum email length (RFC 5321).
/// </summary>
public const int EmailMaxLength = UserConstants.MaxEmailLength;

/// <summary>
/// The production maximum username length.
/// </summary>
public const int UserNameMaxLength = UserConstants.MaxUserNameLength;

/// <summary>
/// The production minimum username length.
/// </summary>
public const int UserNameMinLength = UserConstants.MinUserNameLength;

/// <summary>
/// The production minimum password length.
/// </summary>
public const int PasswordMinLength = UserConstants.MinPasswordLength;

/// <summary>
/// Test-owned. Production declares no maximum password length, so this value binds
/// nothing in <c>src/</c>; a test that treats it as a production limit is asserting
/// against a number this file invented.
/// </summary>
public const int PasswordMaxLength = 128;

/// <summary>
/// The production maximum country name length.
/// </summary>
public const int CountryMaxLength = UserConstants.MaxCountryNameLength;

/// <summary>
/// The production maximum full phone number length.
/// </summary>
public const int PhoneMaxLength = UserConstants.MaxFullPhoneNumberLength;
```

The same "no production counterpart" note applies to `Role.NameMinLength`,
`Role.DescriptionMinLength`, `Permission.ResourceMinLength`,
`Permission.ActionMinLength` and `Permission.DescriptionMinLength`. Verified: neither
`RoleConstants` nor `PermissionConstants` declares a minimum, and
`RoleValidation.ValidRoleName` / `ValidRoleDescription`
(`src/Modules/Identity/Identity/Application/Auth/Validators/RoleValidation.cs:32, 63`)
and `PermissionValidation.ValidPermissionResource`
(`.../PermissionValidation.cs:32`) enforce a maximum and a not-empty rule only. Leave the
values as they are, document them as test-owned, and note in the PR that a test asserting
"invalid below the minimum" for a role name is asserting a rule production does not have.

Also update the class-level doc comments. The current wording — "Mirrors
`src/BuildingBlocks/Constants/UserConstants.cs`" — is what made copying look correct.
Replace it with a statement of the actual relationship, as in the `Otp` example above.

`TestConstants.Jwt.cs` keeps its values, and gains a note that they must stay identical
to what `ApiFixture.SetEnvironmentVariables` exports at `ApiFixture.cs:77-79`, because
[01-test-host-fidelity.md](01-test-host-fidelity.md) Change 4 makes the host validate
with those exported values.

**If this is done wrong** — if a value is aliased to the wrong production constant, for
example `Session.UserAgentMaxLength` to `MaxRefreshTokenHashLength`, which is also 500 —
the tests stay green and the link is a lie. Check each alias against the production file,
not against the number.

### 2. Assert both sides of the OTP lockout boundary

File: `tests/Unit/Modules/Identity/Domain/Entities/OtpEntityTests.cs`.

```csharp
// tests/Unit/Modules/Identity/Domain/Entities/OtpEntityTests.cs:188-202 — before
[Fact]
public void HasMaxAttemptsReached_AfterIncrementingToMax_ShouldReturnTrue()
{
    // Arrange
    OtpEntity otp = OtpFactory.Create();

    // Act - Increment to max attempts (5 by default)
    for (int i = 0; i < TestConstants.Otp.MaxAttempts; i++)
    {
        otp.IncrementAttemptCount();
    }

    // Assert
    otp.HasMaxAttemptsReached().Should().BeTrue();
}
```

Production is `return AttemptCount >= UserConstants.MaxOtpAttempts;`
(`src/Modules/Identity/Identity/Domain/Entities/OtpEntity.cs:107-109`). Five increments
satisfy `>= 1` through `>= 5` alike, so the test passes for every threshold the constant
could hold. The inline comment "(5 by default)" is wrong about the system under test.

```csharp
// after
/// <summary>
/// One attempt below the threshold must not lock the OTP. This is the half of the
/// boundary that discriminates: without it, the assertion below passes for any
/// threshold at or under the number of increments performed.
/// </summary>
[Fact]
public void HasMaxAttemptsReached_OneAttemptBelowTheThreshold_ShouldReturnFalse()
{
    OtpEntity otp = OtpFactory.Create();

    for (int i = 0; i < TestConstants.Otp.MaxAttempts - 1; i++)
    {
        otp.IncrementAttemptCount();
    }

    otp.HasMaxAttemptsReached().Should().BeFalse();
}

/// <summary>
/// The threshold itself locks the OTP, which is the brute-force control on a six-digit
/// keyspace.
/// </summary>
[Fact]
public void HasMaxAttemptsReached_AtTheThreshold_ShouldReturnTrue()
{
    OtpEntity otp = OtpFactory.Create();

    for (int i = 0; i < TestConstants.Otp.MaxAttempts; i++)
    {
        otp.IncrementAttemptCount();
    }

    otp.HasMaxAttemptsReached().Should().BeTrue();
}
```

Keep `HasMaxAttemptsReached_WhenBelowMax_ShouldReturnFalse` (`:162-173`) — it asserts the
zero-attempt case, which the new pair does not cover — and keep
`HasMaxAttemptsReached_WhenAtMax_ShouldReturnTrue` (`:175-186`), which covers the builder
path rather than the increment path.

**If this is done wrong** — if only the "at the threshold" case is kept — the test still
passes for any threshold at or below the loop count, which is the exact defect being
fixed.

### 3. Make `AsMaxAttemptsReached()` produce a reachable state

File: `tests/Fixtures/Builders/Entities/Identity/OtpBuilder.cs`.

```csharp
// tests/Fixtures/Builders/Entities/Identity/OtpBuilder.cs:139-147 — before
/// <summary>
/// Sets the OTP as having reached max attempts.
/// </summary>
/// <returns>The builder instance for chaining.</returns>
public OtpBuilder AsMaxAttemptsReached()
{
    _attemptCount = TestConstants.Otp.MaxAttempts;
    return this;
}
```

```csharp
// after
/// <summary>
/// Sets the OTP to the highest attempt count production can persist. The entity stops
/// accepting attempts once the count reaches the threshold, so this is the locked state
/// a real row holds — not one above it.
/// </summary>
/// <returns>The builder instance for chaining.</returns>
public OtpBuilder AsMaxAttemptsReached()
{
    _attemptCount = TestConstants.Otp.MaxAttempts;
    return this;
}
```

The expression is unchanged; the alias from Change 1 is what fixes it. Before the alias
this produced `AttemptCount == 5`, a state production can never reach, and `Build()`
reaches it by calling `IncrementAttemptCount()` in a loop (`OtpBuilder.cs:167`). After
the alias it produces 3, which is exactly the locked state. The doc comment change is
the deliverable here: it records why the value is the threshold rather than "max plus
something", so nobody re-inflates it.

Three factories depend on this and need no edit: `OtpFactory.CreateMaxAttemptsReached()`
(`OtpFactory.cs:90`), its user-scoped overload (`:98`) and the code/purpose overload
(`:207`).

**If this is done wrong** — if the builder is "fixed" by hardcoding 3 instead of reading
the aliased constant — the copy problem returns in a new place.

## Expected fallout

**Seven values change. Read this table before running anything.**

| Test constant | Was | Becomes | Production source | Consumers today |
| --- | --- | --- | --- | --- |
| `Otp.MaxAttempts` | 5 | **3** | `UserConstants.MaxOtpAttempts` | 2 (`OtpBuilder.cs:145`, `OtpEntityTests.cs:195`) |
| `Otp.ExpirationMinutes` | 10 | **60** | `UserConstants.OtpExpirationMinutes` | 3 (`OtpBuilder.cs:39`, `OtpEntityTests.cs:26`, `MockOtpService.cs:121`) |
| `User.UserNameMaxLength` | 50 | **20** | `UserConstants.MaxUserNameLength` | 2 (`UserBuilder.cs:39-40`) |
| `User.EmailMaxLength` | 256 | **254** | `UserConstants.MaxEmailLength` | 0 |
| `User.PasswordMinLength` | 8 | **6** | `UserConstants.MinPasswordLength` | 0 |
| `Session.DeviceIdMaxLength` | 256 | **64** | `SessionConstants.MaxDeviceIdLength` | 0 |
| `Session.UserAgentMaxLength` | 512 | **500** | `SessionConstants.MaxUserAgentLength` | 0 |

**Five of the seven drifted upward, and that direction is the dangerous one.** A test
constant *larger* than production means an "invalid at max + 1" assertion was built from
a string longer than production's real edge — the validator rejects it, the test passes,
and the actual boundary at the production limit was never exercised. Nothing about that
failure is visible; the test is green and named as though it covers the edge. A test
constant *smaller* than production fails loudly instead, which is why only the upward
drifts survived long enough to be found.

**The measured consumer counts say where the red will actually appear, and it is
narrower than the direction of the drift suggests.** The Identity length boundaries are
already written against the production constants directly —
`AdminLoginValidatorTests.cs:52` and `:129` use `UserConstants.MaxEmailLength`,
`CredentialValidationTests.cs:361, 407, 490` and `PublicSignUpValidatorTests.cs:141` use
`UserConstants.MaxUserNameLength` — which is why four of the seven drifted constants have
no consumers at all. Aliasing them changes no assertion today; it closes the hole before
the next author reaches for `TestConstants.User.EmailMaxLength` and writes a test that
proves nothing.

Where a test does turn red, expect it here:

- **`OtpEntityTests` and anything reading `Otp.ExpirationMinutes`.** The builder default
  expiry moves from ten minutes to sixty. Assertions about an OTP being valid still
  hold; assertions built on "expires soon" may not.
- **`UserBuilder`-generated usernames become 20 characters instead of 50.** Any test
  asserting on a generated username's length or content will see a different value. The
  contradiction this fixes is visible in a single method: the comment at
  `UserBuilder.cs:36` says "fits within the max length (20 chars)" while the code
  truncates at 50.
- **`OtpRepositoryTests`** already hardcodes the real threshold in a comment
  (`:171` — "One less than max (max = 3)"). It should keep passing, and it is the
  cross-check that 3 is right.

**The rule, without exception: fix the test, never the constant.** Every red result here
is a boundary that was not being checked. Widening a production limit so a test goes
green would take a defect the audit found and encode it in `src/`. If a production limit
genuinely looks wrong — 6 characters is a short minimum password — that is a product
decision with its own ticket, made deliberately, not a side effect of a test fix.

**Watch for the reverse mistake too.** Five test constants describe minimum lengths that
production does not enforce (`Role.NameMinLength`, `Role.DescriptionMinLength`,
`Permission.ResourceMinLength`, `Permission.ActionMinLength`,
`Permission.DescriptionMinLength`). If a test asserts that a one-character role name is
rejected, it is asserting a rule that does not exist, and it passes only because some
other rule rejects the input. Note any such test in the PR; do not fix it here.

## Testing

```bash
dotnet build
dotnet test tests/Unit
dotnet test tests/Integration
```

The unit suite is where the fallout lands; the integration suite should be unaffected
except through `UserBuilder`-generated data.

Prove the aliases are real rather than coincidental:

```bash
# Every numeric constant in the nine partials either aliases a production constant or
# carries a doc comment saying it is test-owned.
grep -rn "public const int" tests/Fixtures/Constants/
```

Every line the grep returns must be followed by a production identifier or preceded by a
"test-owned" doc comment. There is no automated form of this check; it is a read-through
of a short file set, once.

Prove the lockout test can now fail:

1. Change `UserConstants.MaxOtpAttempts` to 4 locally.
2. `HasMaxAttemptsReached_OneAttemptBelowTheThreshold_ShouldReturnFalse` and
   `HasMaxAttemptsReached_AtTheThreshold_ShouldReturnTrue` must both still pass, because
   they read the same constant the production code reads.
3. Change `OtpEntity.HasMaxAttemptsReached` to `AttemptCount > UserConstants.MaxOtpAttempts`.
4. `HasMaxAttemptsReached_AtTheThreshold_ShouldReturnTrue` must fail. Before this spec it
   did not.
5. Revert both edits.

That sequence is the whole point of the spec: the test now tracks the production
threshold and still discriminates the comparison operator.

## Risks

**An alias points at a plausible but wrong production constant.** Several production
constants share values — `MaxUserAgentLength` and `MaxRefreshTokenHashLength` are both
500, `MaxPermissionResourceLength` and `MaxPermissionActionLength` are both 15 — so a
mis-aliased constant compiles and passes. Mitigation: the mapping table above was built
by reading each production file; re-check each alias against the source file rather than
against the number, and treat a value match as no evidence at all.

**A red test gets fixed by editing the constant.** This is the failure mode the spec
exists to prevent. Mitigation: state it in the PR description, and have the reviewer
check that no file under `src/BuildingBlocks/Constants/` or
`src/Modules/Content/Content/Domain/Constants/` appears in the diff. If one does, the
change is out of scope.

**`Content` constants may look drifted when they are not.** Every value in
`TestConstants.Content.cs` was checked against `ContentConstants` and none has drifted,
so aliasing that file changes nothing at runtime. It is still worth doing — it is the
largest partial and the most exposed to future drift — but do not go looking for a
failure there to justify the work.

**The `Editorial` sub-classes share production constants.** `Article.TitleMaxLength`,
`Video.TitleMaxLength`, `Article.SlugMaxLength`, `Video.SlugMaxLength`,
`ShortVideo.SlugMaxLength`, `Lyrics.SlugMaxLength` and `Artist.SlugMaxLength` all alias
`ContentConstants.MaxTitleLength` and `MaxSlugLength`, because production applies one
limit across the editorial types. Keep the per-type test constants rather than collapsing
them: they document which column each assertion is about, and if production ever splits
the limits the aliases are the only thing that has to change.

## Implementation notes

Implemented 2026-08-22, first in the executed order because several builder methods
bake the drifted values in.

**There are 29 `TestConstants` partials, not nine.** The audit counted the nine that
held drifted numeric limits; the file set is 29 across `Content/`, `Core/`, `Identity/`
and `Shared/`. The aliasing rule was applied to all of them rather than only the nine,
since a partial that aliases nothing today is exactly where the next drift starts.

The doc-comment convention landed as two mutually exclusive forms, so a reader can
tell aliased from owned without opening `src/`:

- an aliased value states what production calls it — *"The production maximum username
  length."* over `public const int UserNameMaxLength = UserConstants.MaxUserNameLength;`
- an owned value says so and says why nothing binds it — *"Test-owned. Production
  declares no maximum password length, so this value binds nothing in `src/`."*

No "Mirrors `src/...`" comment survives anywhere under `tests/Fixtures/Constants/`.
`TestConstants.Jwt` carries the sync note against `ApiFixture.SetEnvironmentVariables`
as specified.

Change 2 landed as two added facts rather than an edit to the existing ones:
`HasMaxAttemptsReached_OneAttemptBelowTheThreshold_ShouldReturnFalse` and
`HasMaxAttemptsReached_AtTheThreshold_ShouldReturnTrue` both drive
`TestConstants.Otp.MaxAttempts` through real `RecordAttempt()` calls, so the boundary
moves with production rather than with a literal.

`OtpBuilder.AsMaxAttemptsReached()` is documented as *"the attempt threshold, the
locked state a real row holds"* — the drift the audit found (a count production cannot
reach) is gone.

## Checklist

- [x] 1 — All 29 `TestConstants` partials alias production constants where one exists
- [x] 1 — Every remaining literal carries a doc comment stating it is test-owned and why
- [x] 1 — The "Mirrors `src/...`" doc comments replaced with a statement of the real
      relationship
- [x] 1 — `TestConstants.Jwt` notes that its values must match
      `ApiFixture.SetEnvironmentVariables`
- [x] 2 — `OtpEntityTests` asserts both sides of the lockout boundary
- [x] 3 — `OtpBuilder.AsMaxAttemptsReached()` documented as the highest persistable count
- [x] Every test that turned red was fixed as a test; no file under
      `src/**/Constants/` appears in the diff
- [x] Full unit suite green; full integration suite green
