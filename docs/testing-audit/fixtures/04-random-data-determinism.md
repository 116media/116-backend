# Medium — One seeded `Random` is shared by 73 `Faker` instances across parallel tests

A module initializer seeds Bogus's global randomizer with a fixed value and documents the
result as reproducible test data. 73 fixture files then declare their own `Faker`, and
every one of them draws from that single `System.Random`. Under xUnit's default
parallelism the draw order depends on thread scheduling, so the promised reproducibility
is not delivered; and `System.Random` is not thread-safe, so concurrent draws can corrupt
its internal state.

## The problem

```csharp
// tests/Fixtures/TestDataModuleInitializer.cs:6-17
/// <summary>
/// Seeds Bogus's global randomizer with a fixed value so generated test data is
/// reproducible across runs. Uniqueness still comes from <see cref="System.Guid" />,
/// which is independent of this seed, so determinism does not reintroduce
/// duplicate-key collisions. To reproduce a specific failure, keep this seed; to
/// explore other data, change it intentionally.
/// </summary>
internal static class TestDataModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize() => Randomizer.Seed = new Random(Seed: 116116);
}
```

`Randomizer.Seed` is a static property on Bogus's `Randomizer`. Assigning it replaces the
one `System.Random` instance that every `Randomizer` — and therefore every `Faker` — draws
from unless it is given its own.

None of them is given its own. 73 fixture files declare the same field:

```csharp
// tests/Fixtures/Builders/Entities/Identity/UserBuilder.cs:15
private readonly Faker _faker = new();
```

| Location | Files declaring `private readonly Faker _faker = new();` |
| --- | --- |
| `Builders/Requests` | 55 |
| `Builders/Entities` | 13 |
| `Builders/Commands` | 4 |
| `Builders/Helpers` | 1 |
| **Total** | **73** |

And the suite runs in parallel. There is no `xunit.runner.json` anywhere under `tests/`,
and no `[assembly: CollectionBehavior]` attribute in `tests/Unit`, so xUnit's default
applies: one collection per test class, collections executed concurrently across worker
threads.

### Consequence 1 — the reproducibility the doc comment promises does not exist

A seeded PRNG is reproducible only if the sequence of draws is reproducible. Here the
sequence is interleaved across every test running at that moment. Test A's third draw is
the global stream's 41st value on one run and its 2,206th on the next, because a different
set of tests happened to be in flight. Re-running with the same seed after a failure
reproduces the *set* of values the suite consumed, not the values any individual test saw.

The instruction in the doc comment — "to reproduce a specific failure, keep this seed" —
is the operation the design cannot support.

### Consequence 2 — `System.Random` is not thread-safe

The `System.Random` documentation is explicit that instance methods are not thread-safe
and that concurrent calls can produce incorrect results. Its internal state is an array
plus two indices that are advanced without synchronisation; a torn update can leave the
generator returning the same value indefinitely, or returning a value outside the range
the caller asked for.

Bogus surfaces that failure a long way from its cause. `_faker.Random.Number(100, 999)`,
`_faker.Internet.Email()` and `_faker.PickRandom(...)` all index into arrays using a
derived draw, so a degenerate value emerges as an `ArgumentOutOfRangeException` or
`IndexOutOfRangeException` thrown inside Bogus's dataset code, in a test whose own stack
frame is a builder constructor. That reads as a flaky fixture, not as a data race.

### What is genuinely bounded

The doc comment's claim that uniqueness comes from `Guid` is true where it was applied.
Ten of the 36 entity builders append a GUID to their unique-constrained defaults:

```csharp
// tests/Fixtures/Builders/Entities/Content/VideoBuilder.cs:19-20
private string _title = $"{TestConstants.Content.Editorial.Video.ValidTitle} {Guid.NewGuid():N}";
private string _slug = $"{TestConstants.Content.Editorial.Video.ValidSlug}-{Guid.NewGuid():N}";
```

The same pattern appears in `ArticleBuilder.cs:16-17`, `ShortVideoBuilder.cs:14-15`,
`LyricsBuilder.cs:20`, `ArtistBuilder.cs:15` and `FileBuilder.cs:34`. For content slugs and
titles the exposure to duplicate-key failures really is closed, and that is worth keeping.

It is not universal. `UserBuilder` derives both of its unique-constrained columns from the
shared randomizer with no GUID component:

```csharp
// tests/Fixtures/Builders/Entities/Identity/UserBuilder.cs:31-43
public UserBuilder()
{
    _id = Guid.NewGuid();
    _email = _faker.Internet.Email().ToLowerInvariant();

    // Generate a username that fits within the max length (20 chars)
    string generatedName = $"{_faker.Name.FirstName()}{_faker.Random.Number(100, 999)}";
    _userName =
        generatedName.Length > TestConstants.User.UserNameMaxLength
            ? generatedName[..TestConstants.User.UserNameMaxLength]
            : generatedName;
    _passwordHash = TestConstants.User.DefaultPasswordHash;
}
```

Both columns carry unique indexes in production:

```csharp
// src/Modules/Identity/Identity/Infrastructure/Persistence/Configurations/UserConfiguration.cs:54-55
builder.HasIndex(u => u.Email).IsUnique();
builder.HasIndex(u => u.UserName).IsUnique();
```

`_faker.Internet.Email()` picks a first name, a surname and a domain from finite datasets.
The username is a first name plus a three-digit number, truncated to 20 characters. Two
integration tests seeding several users each will collide eventually, and when they do the
failure is a `DbUpdateException` on a unique index in a test that was arranging, not
asserting.

## Why it matters

The severity here is bounded and should be read that way: the content builders are
protected, the failure modes are intermittent, and no assertion is weakened by any of it.
What is at stake is trust in intermittent failures.

A suite that produces a `DbUpdateException` on `ix_users_email` once a fortnight, and an
`IndexOutOfRangeException` inside Bogus once a month, teaches its maintainers to re-run
CI rather than investigate. That habit is the actual cost, because it applies to every red
build, including the ones caused by a real defect. A test suite is a signal, and a signal
with unexplained noise is discounted uniformly.

The reproducibility claim compounds it. When a failure does get investigated, the first
move a developer makes is the one the doc comment recommends — re-run with the same seed —
and it does not reproduce, which reinforces the conclusion that the failure was not real.

## The fix

Give each `Faker` its own `Randomizer`, seeded deterministically from a fixed base and a
monotonic counter. Same seed, same per-instance stream, regardless of how the instances
are interleaved.

```csharp
// tests/Fixtures/Helpers/TestFaker.cs — new
using Bogus;

namespace _116.Tests.Fixtures.Helpers;

/// <summary>
/// Creates <see cref="Faker" /> instances that do not share Bogus's process-wide
/// randomizer.
/// </summary>
/// <remarks>
/// Bogus seeds every <see cref="Faker" /> from a single static <see cref="Random" />
/// unless one is supplied. Sharing it makes the value any individual fixture receives
/// depend on how many draws other tests happened to make first, and exposes a
/// non-thread-safe generator to concurrent test collections. Each instance created here
/// owns a stream derived from a fixed base seed and a monotonic counter, so a given
/// fixture sees the same values on every run.
/// </remarks>
public static class TestFaker
{
    private const int BaseSeed = 116116;

    private static int _counter;

    /// <summary>
    /// Creates a <see cref="Faker" /> with a private, deterministically seeded randomizer.
    /// </summary>
    public static Faker Create() =>
        new() { Random = new Randomizer(BaseSeed + Interlocked.Increment(ref _counter)) };
}
```

Then the 73 declarations become one-line edits:

```csharp
// tests/Fixtures/Builders/Entities/Identity/UserBuilder.cs — before
private readonly Faker _faker = new();

// after
private readonly Faker _faker = TestFaker.Create();
```

`TestDataModuleInitializer` stays. It is the backstop for any `Faker` that is created
without going through the helper, and removing it would make those cases non-deterministic
rather than merely order-dependent. Its doc comment needs the qualification the code now
earns:

```csharp
// tests/Fixtures/TestDataModuleInitializer.cs — after
/// <summary>
/// Seeds Bogus's process-wide randomizer as a backstop for any <see cref="Bogus.Faker" />
/// created without <see cref="Helpers.TestFaker.Create" />. Fixtures that use the helper
/// own a private stream and do not depend on this value; a shared stream is
/// order-dependent under parallel execution and cannot be relied on to reproduce a
/// specific failure.
/// </summary>
```

Close the remaining uniqueness gap while the file is open:

```csharp
// tests/Fixtures/Builders/Entities/Identity/UserBuilder.cs — after
public UserBuilder()
{
    _id = Guid.NewGuid();
    _email = $"{_faker.Internet.UserName()}.{Guid.NewGuid():N}@example.test".ToLowerInvariant();

    string suffix = Guid.NewGuid().ToString("N")[..6];
    string generatedName = $"{_faker.Name.FirstName()}{suffix}";
    _userName = generatedName[..Math.Min(generatedName.Length, TestConstants.User.UserNameMaxLength)];

    _passwordHash = TestConstants.User.DefaultPasswordHash;
}
```

That brings `UserBuilder` in line with the ten content builders that already guarantee
uniqueness structurally, and it uses the aliased constant from
[01-constant-drift.md](01-constant-drift.md) so the truncation matches the real column.

## The principle

**Shared mutable state defeats determinism even when it is seeded.** A fixed seed makes a
sequence reproducible; it makes a *consumer's view* of that sequence reproducible only if
the consumer owns the sequence. Under parallelism, ownership is the requirement, not
seeding.

The same reasoning applies to the thread-safety half. A generator with unsynchronised
mutable state is a shared resource, and a test suite that hands one to 73 concurrent
consumers has a data race regardless of how rarely it fires. The fix is not a lock — it is
to stop sharing, which costs one allocation per fixture and removes the question entirely.

## Checklist

- [ ] `TestFaker.Create()` exists and seeds each instance from a fixed base plus
      `Interlocked.Increment`
- [ ] All 73 `private readonly Faker _faker = new();` declarations call it
- [ ] `grep -rn "Faker _faker = new()" tests/` returns nothing
- [ ] `TestDataModuleInitializer` retained as a backstop, with its reproducibility claim
      corrected
- [ ] `UserBuilder` derives email and username with a GUID component, matching the ten
      content builders that already do
