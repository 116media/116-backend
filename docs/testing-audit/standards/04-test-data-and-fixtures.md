# Test data and fixtures standard

How arrangements are built in this suite. Test data is the part of a test that decides what
the assertion is actually about, so a fixture layer that produces unreachable states or
drifted values weakens every test built on it — silently, and everywhere at once.

## 1. The layering rule: builder, factory, inline

Three ways to produce test data, in strict order of preference.

| Layer | Applies when | Lives in |
| --- | --- | --- |
| **Builder** | any shape a test needs — the default, requiring no justification | `tests/Fixtures/Builders/` |
| **Factory** | a shape three or more tests share, verbatim | `tests/Fixtures/Factories/` |
| **Inline construction** | the type under test, in that type's own test file | the test file |

**The builder is the default.** It is a fluent chain the call site can compose, so a test
that needs an approved video with a thumbnail writes that combination instead of asking for
a method that produces it.

**The factory is a named alias for a chain, and nothing more.** Three call sites is the
threshold, and it is a floor, not a target. A factory method with one caller is a
one-line chain with an indirection in front of it — the honest form is the chain.

**Inline construction is for the subject of the test.** `UserEntityTests` constructs a
`UserEntity` directly, because the construction is what it is testing. Every other test
file uses a builder or a factory, because a hand-rolled entity skips the invariants in
rule 3.

The suite currently violates this from both ends: all 36 entity builders are `internal` and
therefore unreachable from any test, and 380 factory methods have grown to compensate, 64
of them uncalled. See
[02-builder-visibility-and-factory-explosion.md](../fixtures/02-builder-visibility-and-factory-explosion.md).

The names are the symptom to watch for in review. When a factory acquires
`CreateApprovedWithYoutubeUrl` and `CreatePublishedForArtist`, the combinatorics have
escaped the chain and moved into the identifier space, where they multiply.

## 2. Constants alias production; they are never copied

A test constant states a production value by reference, never by literal.

```csharp
// bad — an independent copy, free to drift
public static class Otp
{
    public const int MaxAttempts = 5;
    public const int ExpirationMinutes = 10;
}

// good — compile-time alias, propagates on the next build
using _116.BuildingBlocks.Constants;

public static class Otp
{
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

`const` aliasing another `const` is legal C# and remains compile-time, so there is no
runtime cost and no reason not to.

Seven of this suite's copied constants have already drifted, and the failure mode is
asymmetric: a test constant *larger* than production fails loudly and gets investigated,
while a test constant *smaller* than production leaves the real edge permanently
unexercised. 122 boundary assertions are anchored to these numbers. See
[01-constant-drift.md](../fixtures/01-constant-drift.md).

Where a test genuinely needs a value different from production — a shorter expiry to keep a
test fast — that is a fixture concern to be injected, not a constant that claims to mirror
production.

This does not conflict with "never derive the expected value from the system under test"
in [01-unit-testing-standard.md](01-unit-testing-standard.md). A constant is an input to
both sides. An answer computed by the system under test is not.

## 3. Builders drive real domain methods, so invariants hold

A builder reaches its target state by calling the same transitions production calls. It
never assigns a status field, and it never reflects a state into place that the domain
would refuse to produce.

This codebase already does it well, and the pattern is worth quoting as the reference:

```csharp
// tests/Fixtures/Builders/Entities/Content/VideoBuilder.cs:309-337
private void ApplyStatusTransition(VideoEntity entity, VideoErrors errors)
{
    switch (_targetStatus)
    {
        case EnumContentStatus.PendingPayment:
            entity.Submit();
            break;
        case EnumContentStatus.PendingReview:
            entity.MarkPendingReview();
            break;
        case EnumContentStatus.Approved:
            entity.MarkPendingReview();
            entity.Approve();
            break;
        case EnumContentStatus.Published:
            entity.MarkPendingReview();
            entity.Approve();
            entity.Publish(errors);
            break;
        case EnumContentStatus.Rejected:
            entity.Reject(_rejectionReason ?? TestConstants.Content.Editorial.Video.ValidRejectionReason);
            break;
        case EnumContentStatus.Archived:
            entity.MarkPendingReview();
            entity.Approve();
            entity.Publish(errors);
            entity.Archive();
            break;
    }
}
```

`AsPublished()` walks `MarkPendingReview() → Approve() → Publish(errors)`. Every guard on
the path runs, the domain events those methods raise are raised, and the resulting entity
is one the application could have produced. If a future change makes `Publish` reject
un-approved content, this builder fails at the point of arrangement rather than producing a
state no code path can reach.

The counter-example is in the same fixture tree:

```csharp
// tests/Fixtures/Builders/Entities/Identity/OtpBuilder.cs:143-146
public OtpBuilder AsMaxAttemptsReached()
{
    _attemptCount = TestConstants.Otp.MaxAttempts;   // 5; production locks at 3
    return this;
}
```

Production stops accepting attempts at 3, so a row with `AttemptCount == 5` cannot exist.
Every test built on that method asserts against a state the system cannot be in.

The rule in review terms: **if a builder sets a field that the domain owns a method for, it
is wrong.** Fields the domain does not expose are rule 4.

## 4. Reflection reconstitutes persisted state, and always uses `nameof`

A test sometimes needs to start from a row the database wrote and the domain does not let
callers set — `CreatedAt`, `PublishedAt` on a backdated entity, an EF shadow navigation.
That is legitimate, and reflection is the tool.

```csharp
// tests/Fixtures/Builders/Entities/Content/VideoBuilder.cs:289-297
if (_publishedAtOverride.HasValue)
{
    PropertyInfo publishedProp = typeof(VideoEntity).GetProperty(
        nameof(VideoEntity.PublishedAt),
        BindingFlags.Public | BindingFlags.Instance
    )!;

    publishedProp.SetValue(entity, _publishedAtOverride);
}
```

Two properties make this acceptable. It runs *after* `ApplyStatusTransition`, so the entity
is genuinely published and only the timestamp is being backdated. And it uses
`nameof(VideoEntity.PublishedAt)`, so a rename is a compile error.

String literals are not acceptable:

```csharp
// tests/Fixtures/Factories/Content/VideoFactory.cs:134 — a rename silently breaks this
.GetProperty("Category", BindingFlags.Public | BindingFlags.Instance)!
```

Reflection is **never** acceptable to reach a state the domain refuses to produce, and never
to invoke a private method. If the state is genuinely reachable in production, the domain
is missing a transition and `src/` is what needs the change. `tests/Integration/` uses
reflection zero times, which is the correct number for a suite driving real entry points.

## 5. Random data is deterministic and per-instance

Every `Faker` owns its own randomizer. Sharing Bogus's process-wide static `Random` makes
the values a fixture receives depend on how many draws other tests happened to make first,
and exposes a generator that is not thread-safe to parallel test collections.

```csharp
// bad — draws from the global static randomizer
private readonly Faker _faker = new();

// good — private, deterministically seeded stream
private readonly Faker _faker = TestFaker.Create();
```

Fields with a database uniqueness constraint do not rely on the generator at all. They carry
a GUID component, which is independent of any seed:

```csharp
// tests/Fixtures/Builders/Entities/Content/VideoBuilder.cs:19-20
private string _title = $"{TestConstants.Content.Editorial.Video.ValidTitle} {Guid.NewGuid():N}";
private string _slug = $"{TestConstants.Content.Editorial.Video.ValidSlug}-{Guid.NewGuid():N}";
```

Ten of the 36 entity builders already do this. `UserBuilder` does not, and its email and
username columns both carry unique indexes. See
[04-random-data-determinism.md](../fixtures/04-random-data-determinism.md).

The same rule bars wall-clock reads in a builder:

```csharp
// tests/Fixtures/Builders/Entities/Content/VideoBuilder.cs:304
entity.CreatedAt = DateTime.UtcNow;
```

An arrangement whose timestamp changes on every run cannot be asserted against exactly.
Builders take an explicit timestamp, defaulting to a fixed instant, and tests that care
advance it deliberately.

## 6. Mock helpers state what a test depends on; they never hide it

A mock helper's job is to remove ceremony, not to make decisions the test should be making.

**Defaults are for members the test does not care about** — void returns, fire-and-forget
writes, `Task.CompletedTask`. They are never for a read whose answer the assertion depends
on, because a value the test never chose cannot be a value the test is checking.

```csharp
// bad — installed before the test says anything; every lookup is a miss
mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync((ArticleEntity?)null);

// good — the test names the id it is arranging a miss for
mock.Setup(x => x.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
    .ReturnsAsync((ArticleEntity?)null);
```

**Setup helpers take the arguments they match on.** A helper that accepts the entity but
matches `It.IsAny<Guid>()` leaves the test unable to distinguish a handler asking for the
right entity from one asking for the wrong one, even when the author wants to.

**A helper's default is the answer that makes a wrong implementation fail.** For a lookup
that is "not found". For a credential check that is "rejected" — the password service mock
currently defaults `Verify` to `true`, i.e. a service that accepts every credential.

See [05-mock-defaults-and-dead-helpers.md](../fixtures/05-mock-defaults-and-dead-helpers.md).

## 7. Dead fixtures get deleted

Unused fixture code is not free. It makes the live surface unsearchable, and an author who
cannot find the helper writes their own — which is why the suite has 188 raw `new Mock<>`
instances across 70 files, 62 of them re-implementing `FileTestHelpers.CreateMockFormFile`.

Current dead surface:

| Surface | Total | Never called |
| --- | --- | --- |
| Public members in `tests/Fixtures/` | 1,152 | 275 |
| Static factory methods | 380 | 64 |
| Helper methods in `tests/Unit/Common/Mocks/` | 546 | 108 |

The deletion rule is mechanical and should run as part of any fixture change: a public
fixture member with no call site is deleted in the same pull request that made it
unreferenced. A helper kept "in case someone needs it" is a helper nobody can find.

## Review checklist

- [ ] New arrangement uses a builder; a new factory method has three or more call sites
- [ ] Any production value referenced by the fixture is aliased, not copied
- [ ] Builders reach every state through real domain methods — no status field assignment
- [ ] Reflection, if present, reconstitutes persisted state and uses `nameof`
- [ ] `Faker` instances come from `TestFaker.Create()`; unique-constrained fields carry a
      GUID component
- [ ] No `DateTime.UtcNow` inside a builder
- [ ] Mock defaults cover write and void members only; read setups take the identifier
- [ ] Every fixture member added has a caller; every member left without one is deleted
