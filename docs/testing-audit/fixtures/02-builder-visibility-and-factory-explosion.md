# High — Every entity builder is `internal`, and 380 factory methods exist to work around it

All 36 entity builders under `tests/Fixtures/Builders/Entities/` are declared `internal`,
and the fixtures project publishes no `InternalsVisibleTo`. No test in either test
project can name one. The 380 static factory methods that wrap them are not a design
choice — they are the only door into the fixture data, and their names have grown
combinatorial because a caller who cannot chain must be handed a pre-chained result.

## The problem

Every entity builder is invisible outside its own assembly:

```csharp
// tests/Fixtures/Builders/Entities/Content/VideoBuilder.cs:10-14
/// <summary>
/// Fluent builder for creating <see cref="VideoEntity"/> instances in tests.
/// For test code, prefer using VideoFactory instead of direct Builder usage.
/// </summary>
internal class VideoBuilder
```

```csharp
// tests/Fixtures/Builders/Entities/Identity/UserBuilder.cs:13
internal class UserBuilder
```

The doc comment says "prefer". The access modifier says "cannot". All 36 files under
`tests/Fixtures/Builders/Entities/` declare `internal class`, and
`tests/Fixtures/_116.Tests.Fixtures.csproj` contains no `InternalsVisibleTo` item — the
whole file is 26 lines of `TargetFramework`, three package references and six project
references.

The measurement follows from the modifier:

```
grep -rl "Builders.Entities" tests/Unit tests/Integration --include="*.cs"   → 0 files
grep -rl "Builders.Requests" tests/Unit tests/Integration --include="*.cs"   → 71 files
grep -rl "Builders.Commands" tests/Unit tests/Integration --include="*.cs"   → 3 files
```

`Builders/Requests` and `Builders/Commands` hold `public` builders and are used directly
by tests. `Builders/Entities` holds `internal` builders and is used by nothing. The two
sibling folders differ in exactly one keyword, and that keyword decides whether 284
fluent methods are reachable.

The asymmetry is visible from `src/`, which grants the unit test project access to its
own internals in three places:

```xml
<!-- src/Modules/Identity/Identity/Identity.csproj:13 -->
<InternalsVisibleTo Include="_116.Unit.Tests" />
```

The same line appears at `src/Modules/Content/Content/Content.csproj:13` and
`src/Modules/Mailer/Mailer/Mailer.csproj:13`. Production code opens itself to the tests;
the test fixtures do not.

### The consequence: 380 factory methods for 284 builder methods

| Surface | Count |
| --- | --- |
| Entity builder files | 36 (all `internal`) |
| Public members on those builders | 342 |
| …of which fluent methods returning the builder | 284 |
| Factory files | 43 |
| Static factory methods | 380 |

Because the caller cannot write `new VideoBuilder(categoryId).AsApproved().WithYoutubeUrl().Build()`,
the factory must ship that exact combination as a named method. `VideoFactory` has 25 of
them, and the names record the chains they replace:

```csharp
// tests/Fixtures/Factories/Content/VideoFactory.cs:71
public static VideoEntity CreatePublishedForArtist(Guid categoryId, Guid artistId) => ...

// tests/Fixtures/Factories/Content/VideoFactory.cs:123
public static VideoEntity CreateApprovedWithYoutubeUrl(Guid categoryId) => ...
```

Each new axis multiplies rather than adds. `Published × Artist`, `Approved × YoutubeUrl`,
`Paid × PendingPayment`, `Promoted × PromotionLevel` — every pairing a test needs becomes
a method, and every method needs a doc comment, a name nobody can guess, and a place in
a file that grows without bound.

### The same body under two names

```csharp
// tests/Fixtures/Factories/Content/ArticleFactory.cs:12-20
/// <summary>
/// Creates a free article in Draft status with the given category.
/// </summary>
public static ArticleEntity Create(Guid categoryId) => new ArticleBuilder(categoryId).Build();

/// <summary>
/// Creates a free article in Draft status.
/// </summary>
public static ArticleEntity CreateFree(Guid categoryId) => new ArticleBuilder(categoryId).Build();
```

Byte-identical expressions. `ArticleFactory.Create(` has 165 call sites;
`ArticleFactory.CreateFree` has zero. The second method exists because "free" reads as a
meaningful distinction at the call site — but the builder default already is free, so the
name documents nothing and the method is dead.

### Dead surface

Applying the same test to every factory method:

- **64 of 380** static factory methods are never invoked as `Factory.Method` from any
  file in `tests/Unit` or `tests/Integration`.
- **26 of those** have a name that is never invoked anywhere in the suite under any
  qualification — `VideoFactory.CreateWithYoutubeUrl` (`:21`),
  `OtpFactory.CreateWithCode` (`:146`), `PermissionFactory.CreateCrud` (`:106`),
  `LyricsFactory.CreatePaidWithPromotion` (`:176`), and 22 more.

Across the whole fixtures project, 275 of 1,152 public member declarations are never
referenced by name from either test project. 209 of those 275 sit on the entity builders,
where they are unreachable by construction.

## Why it matters

**A test author cannot express a shape the factory did not anticipate.** The options are
to add a 381st factory method, or to build the entity inline and lose the invariants the
builder enforces. The first grows the surface; the second is worse, because
`VideoBuilder.ApplyStatusTransition` (`VideoBuilder.cs:309-337`) reaches `Published` by
calling `MarkPendingReview() → Approve() → Publish(errors)` on the real entity. An inline
construction skips those calls and produces a state the domain cannot reach — which is
the defect already documented in
[01-constant-drift.md](01-constant-drift.md) for `OtpBuilder.AsMaxAttemptsReached()`.

**Naming carries the combinatorics.** `CreateApprovedWithYoutubeUrl` is not discoverable.
An author who wants an approved video with a thumbnail must read the whole factory file
to find out whether `CreateApprovedWithThumbnail` exists, and if it does not, must decide
between adding it and reaching for something close enough. "Close enough" is how a test
ends up asserting against a state it did not intend to arrange.

**Dead code accumulates invisibly.** Nobody deletes a factory method, because nobody can
tell whether it is the one method some distant test depends on. 64 methods have already
accumulated, each with a doc comment asserting a purpose it does not serve.

**The 14% dead rate is a floor, not a ceiling.** Every method that survives only because
it is hard to prove dead is a maintenance cost with no test behind it.

## The fix

### 1. Make the builders `public`

```csharp
// tests/Fixtures/Builders/Entities/Content/VideoBuilder.cs — before
/// <summary>
/// Fluent builder for creating <see cref="VideoEntity"/> instances in tests.
/// For test code, prefer using VideoFactory instead of direct Builder usage.
/// </summary>
internal class VideoBuilder

// after
/// <summary>
/// Fluent builder for creating <see cref="VideoEntity"/> instances in tests.
/// Drives the real domain transitions, so every state it produces is a state the
/// application can reach. Use it directly for any one-off shape; reach for
/// <see cref="Factories.Content.VideoFactory"/> only for a shape three or more
/// tests share.
/// </summary>
public class VideoBuilder
```

36 one-word edits. `InternalsVisibleTo` is the alternative and is the wrong one: it makes
the fixtures assembly's *entire* internal surface visible to two named assemblies, which
is a weaker statement than "this type is part of the fixture API". The builders are the
fixture API. Say so with the modifier.

### 2. Delete the dead factory methods

The 64 uncalled methods go, starting with the byte-identical duplicate:

```csharp
// tests/Fixtures/Factories/Content/ArticleFactory.cs — before
public static ArticleEntity Create(Guid categoryId) => new ArticleBuilder(categoryId).Build();
public static ArticleEntity CreateFree(Guid categoryId) => new ArticleBuilder(categoryId).Build();

// after
/// <summary>
/// Creates a free article in Draft status — the builder default, which is what the
/// overwhelming majority of article tests arrange.
/// </summary>
public static ArticleEntity Create(Guid categoryId) => new ArticleBuilder(categoryId).Build();
```

### 3. State the layering rule and hold new code to it

The rule that keeps the surface from regrowing:

| Layer | When it applies |
| --- | --- |
| **Builder** | Any shape a test needs. This is the default and needs no justification. |
| **Factory** | A shape three or more tests share, verbatim. The factory is a named alias for a chain, nothing more. |
| **Inline construction** | Only for the type under test itself, in that type's own test file. |

Applied to `VideoFactory`, the 25 methods collapse to a handful. Everything else is one
readable line at the call site:

```csharp
// before — the factory must own every combination
VideoEntity video = VideoFactory.CreateApprovedWithYoutubeUrl(categoryId);
VideoEntity promoted = VideoFactory.CreatePublishedForArtist(categoryId, artistId);

// after — the call site owns its own combination
VideoEntity video = new VideoBuilder(categoryId).AsApproved().WithYoutubeUrl().Build();
VideoEntity promoted = new VideoBuilder(categoryId).AsPublished().WithArtist(artistId).Build();
```

The second form is longer by a few characters and shorter by one indirection: the reader
learns the arranged state from the test, not from a factory file in another folder.

## The principle

**A fixture's API surface is decided by what test authors need to express, not by what a
factory author anticipated.** A builder that cannot be reached forces every future shape
through a maintainer, and maintainers respond by adding names. Names multiply; chains
compose.

The corollary is a deletion rule. A factory method with fewer than three call sites is
not an abstraction, it is a duplicate of one line of builder chain, and the honest form
of that line is the line itself.

## Checklist

- [ ] All 36 entity builders declared `public`
- [ ] No `InternalsVisibleTo` added to `_116.Tests.Fixtures.csproj` — visibility is
      expressed per type
- [ ] The 64 uncalled factory methods deleted, starting with `ArticleFactory.CreateFree`
- [ ] Remaining factory methods have three or more call sites, or are deleted and their
      chain inlined
- [ ] Builder doc comments state the layering rule instead of the "prefer the factory"
      instruction that the access modifier was actually enforcing
- [ ] Full suite green — no test referenced the deleted methods, so it should be
