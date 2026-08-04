# Critical — Test constants are copies of production constants, and seven have drifted

Nine `TestConstants` partials declare in their own doc comments that they "mirror" a
production constants file. They are literal copies with no compile-time link. Seven
values are now wrong. One of them silently disables a security boundary test.

## The proof

```csharp
// tests/Fixtures/Constants/Identity/TestConstants.Otp.cs:11-13
public const int CodeLength = 6;
public const int MaxAttempts = 5;          // production: 3
public const int ExpirationMinutes = 10;   // production: 60
```

```csharp
// src/BuildingBlocks/Constants/UserConstants.cs:88,93,98
public const int OtpExpirationMinutes = 60;
public const int MaxOtpAttempts = 3;
public const int OtpCodeLength = 6;
```

## Consequence 1 — the OTP lockout threshold is untested

```csharp
// tests/Unit/Modules/Identity/Domain/Entities/OtpEntityTests.cs:187-201
[Fact]
public void HasMaxAttemptsReached_AfterIncrementingToMax_ShouldReturnTrue()
{
    OtpEntity otp = OtpFactory.Create();

    // Act - Increment to max attempts (5 by default)
    for (int i = 0; i < TestConstants.Otp.MaxAttempts; i++)   // loops 5 times
    {
        otp.IncrementAttemptCount();
    }

    otp.HasMaxAttemptsReached().Should().BeTrue();
}
```

Production is `return AttemptCount >= UserConstants.MaxOtpAttempts;` — that is
`>= 3`. Five increments satisfy `>= 1`, `>= 2`, `>= 3`, `>= 4` and `>= 5` alike.

**This test passes for every possible threshold value.** It is the only test of the
OTP brute-force lockout, which is the control preventing an attacker from walking
a six-digit keyspace. The inline comment "(5 by default)" is factually wrong about
the system it is testing.

## Consequence 2 — a builder mints a state production cannot produce

```csharp
// tests/Fixtures/Builders/Entities/Identity/OtpBuilder.cs:143-146
public OtpBuilder AsMaxAttemptsReached()
{
    _attemptCount = TestConstants.Otp.MaxAttempts;   // 5
    return this;
}
```

Production locks the OTP at 3 and stops accepting attempts, so a row with
`AttemptCount == 5` can never exist. Every test built on `AsMaxAttemptsReached()`
asserts against a fictional state.

## The other five drifts

| Test constant | Test value | Production value | Production source |
| --- | --- | --- | --- |
| `Otp.MaxAttempts` | 5 | 3 | `UserConstants.MaxOtpAttempts` |
| `Otp.ExpirationMinutes` | 10 | 60 | `UserConstants.OtpExpirationMinutes` |
| `Session.DeviceIdMaxLength` | 256 | 64 | `SessionConstants.MaxDeviceIdLength` |
| `Session.UserAgentMaxLength` | 512 | 500 | `SessionConstants.MaxUserAgentLength` |
| `User.UserNameMaxLength` | 50 | 20 | `UserConstants.MaxUserNameLength` |
| `User.PasswordMinLength` | 8 | 6 | `UserConstants.MinPasswordLength` |
| `User.EmailMaxLength` | 256 | 254 | `UserConstants.MaxEmailLength` |

The username drift is visible as a contradiction inside a single method — the
author knew the real limit and wrote the copied one:

```csharp
// tests/Fixtures/Builders/Entities/Identity/UserBuilder.cs:36-41
// Generate a username that fits within the max length (20 chars)
string generatedName = $"{_faker.Name.FirstName()}{_faker.Random.Number(100, 999)}";
_userName = generatedName.Length > TestConstants.User.UserNameMaxLength   // 50, not 20
    ? generatedName[..TestConstants.User.UserNameMaxLength]
    : generatedName;
```

## Why this pattern is worse than it looks

122 boundary assertions across the unit suite are written as
`new string('a', TestConstants.X.YMaxLength)` and its `+ 1` sibling. Every one is
anchored to a number that is only correct until someone edits `src/` and does not
remember that a copy exists in `tests/Fixtures/Constants/`.

The failure mode is asymmetric, which is what makes it dangerous:

- **Test constant larger than production** — the "valid at max" test fails loudly.
  Annoying, but safe: someone investigates.
- **Test constant smaller than production** — the "invalid at max + 1" test passes
  while the real edge is never exercised. Silent, permanent blind spot.

## The fix

Alias the production constant instead of copying its value. The test projects
already reference `_116.BuildingBlocks` — production constants are used directly
from tests 104 times, so the precedent exists.

```csharp
// tests/Fixtures/Constants/Identity/TestConstants.Otp.cs — before
public static class Otp
{
    public const int CodeLength = 6;
    public const int MaxAttempts = 5;
    public const int ExpirationMinutes = 10;
}

// after
using _116.BuildingBlocks.Constants;

public static class Otp
{
    /// <summary>
    /// The production OTP code length, aliased rather than copied so a change in
    /// <see cref="UserConstants" /> reaches every boundary test at compile time.
    /// </summary>
    public const int CodeLength = UserConstants.OtpCodeLength;

    /// <summary>
    /// The production brute-force lockout threshold.
    /// </summary>
    public const int MaxAttempts = UserConstants.MaxOtpAttempts;

    /// <summary>
    /// The production OTP validity window in minutes.
    /// </summary>
    public const int ExpirationMinutes = UserConstants.OtpExpirationMinutes;
}
```

`const` aliasing another `const` is legal C# and is still compile-time — no runtime
cost, and a value change in `src/` propagates on the next build.

Then rewrite the boundary test the alias will now expose. A threshold test must
assert **both** sides of the boundary, or it is not testing a boundary:

```csharp
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

## The principle

**A test must never restate a production value; it must reference it.** The moment
a number is written down twice, the two copies are independent and one of them is
free to become a lie. This is the same reasoning that makes route constants shared
rather than re-typed — a discipline this codebase already applies rigorously to
routes (1,401 references, zero hardcoded URLs) and abandoned for numbers.

Where a test genuinely needs a *different* value than production — say, a shorter
expiry to keep a test fast — that is a test fixture concern and should be injected,
not redefined as a constant that claims to mirror production.

## Checklist

- [ ] All nine `TestConstants` partials alias production constants
- [ ] `OtpEntityTests` asserts both sides of the lockout boundary
- [ ] `OtpBuilder.AsMaxAttemptsReached()` produces a reachable state
- [ ] Full unit suite green (expect failures from the length drifts — each is a
      real boundary that was never being tested)
