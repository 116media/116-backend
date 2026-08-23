# Spec 08 — Fixture architecture

## Goal

All 36 entity builders under `tests/Fixtures/Builders/Entities/` are declared
`internal` and the fixtures project publishes no `InternalsVisibleTo`, so no test in
either project can name one. The 380 static factory methods that wrap them are not a
design choice — they are the only door into the fixture data, and their names have
grown combinatorial because a caller who cannot chain must be handed a pre-chained
result. This spec makes the builders `public`, deletes the dead surface that
accumulated behind the closed door, writes the builder-factory-inline layering rule
into the fixtures project itself, and moves the 170 reflection writes in
`tests/Unit` onto builder methods so that tests stop constructing states the domain
refuses to produce.

## Scope

In scope:

- The `internal` → `public` change on all 36 files under
  `tests/Fixtures/Builders/Entities/`.
- Deletion of the dead fixture surface, including the 64 uncalled factory methods
  and the byte-identical `ArticleFactory.CreateFree` alias.
- The layering rule, recorded in the fixtures project as a doc comment on the
  builder base and on the factory files, not only in this doc set.
- The two bare-string reflection sites in `tests/Fixtures/Factories/`.
- The 170 reflection property writes across 32 files in `tests/Unit`.

Not in this spec:

- `TestConstants` aliasing production constants and the `OtpBuilder.AsMaxAttemptsReached`
  drift. That is spec 03, and it lands first because it changes what several builder
  methods produce.
- Per-instance `Faker` randomizers and the GUID-suffix rule for unique-constrained
  fields. That is spec 02's change set.
- `DateTime.UtcNow` inside builders. That is spec 09, which needs the builders to be
  `public` first so a test can supply an instant.
- Mock helper files under `tests/Unit/Common/Mocks/`. Those are spec 07; this spec
  covers `tests/Fixtures/` only.
- The `MetaField` decision. See below.

## Prerequisites

- **Spec 03 (constant aliasing)** must land first. It fixes the drifted constants
  that several builder methods bake in, including
  `OtpBuilder.AsMaxAttemptsReached()`, which sets `AttemptCount` to a value
  production cannot reach. Making builders public before that fix advertises a
  broken state to every test author.
- **Spec 05 (outcome assertions)** should land first where it overlaps. Its handler
  rewrites reference factory methods, and doing that against a factory surface that
  is about to shrink means editing the same lines twice.

## Decision deferred

The index records one open decision against this spec:

> Delete `MetaField` init-tests for Mailer (added recently) as the Content/Identity
> equivalents were in July — **Keep for now; revisit with the team.**

That is **deferred, not settled.** The Mailer `MetaField` init-tests live at
`tests/Unit/Modules/Mailer/Application/Newsletter/MetaFields/` and
`tests/Unit/Modules/Mailer/Application/Notifications/MetaFields/`. They stay exactly
as they are under this spec. They were deliberately removed once for Content and
Identity, so re-deciding unilaterally in either direction is the wrong move; record
the outcome here when the team decides, and do not treat the deletion sweep in
change 2 as licence to touch them.

## Changes

### 1. Make the 36 entity builders `public`

**Files:** all 36 files under `tests/Fixtures/Builders/Entities/`.

```csharp
// tests/Fixtures/Builders/Entities/Content/VideoBuilder.cs:10-19 — before
/// <summary>
/// Fluent builder for creating <see cref="VideoEntity"/> instances in tests.
/// For test code, prefer using VideoFactory instead of direct Builder usage.
/// </summary>
internal class VideoBuilder
```

```csharp
// after
/// <summary>
/// Fluent builder for creating <see cref="VideoEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is a state the
/// application can reach.
/// </summary>
/// <remarks>
/// This is the default way to arrange a video. Use it directly for any shape a test
/// needs; reach for <see cref="Factories.Content.VideoFactory" /> only when three or
/// more tests share the same chain verbatim.
/// </remarks>
public class VideoBuilder
```

`tests/Fixtures/Builders/Entities/Identity/UserBuilder.cs:13` takes the same edit,
and so do the remaining 34. The doc comment change matters as much as the keyword:
the old text says "prefer the factory" while the access modifier was enforcing
"you have no choice", and leaving the sentence in place would preserve the wrong
instruction under the new visibility.

**Do not add `InternalsVisibleTo` to `tests/Fixtures/_116.Tests.Fixtures.csproj`.**
That would expose the fixtures assembly's entire internal surface to two named
assemblies, which is a weaker statement than "this type is part of the fixture API".
The builders are the fixture API; say so with the modifier. The 26-line project file
stays as it is.

*If done wrong:* granting `InternalsVisibleTo` instead makes the change invisible in
each type's own declaration, so the next builder added lands `internal` again and
nobody notices for a year.

### 2. Delete the dead fixture surface

**Files:** `tests/Fixtures/Factories/**`, and the builder files after change 1.

The audit measured 275 of 1,152 public member declarations in `tests/Fixtures` as
never referenced by name from either test project, of which 209 sit on the entity
builders where they are unreachable by construction. Change 1 makes those 209
reachable, so the deletion candidate set is not simply "the 275".

The candidate set is roughly 116 members:

- **64 uncalled static factory methods**, of which 26 have a name that appears
  nowhere in the suite under any qualification. Named starting points:
  `VideoFactory.CreateWithYoutubeUrl` (`:21`), `OtpFactory.CreateWithCode` (`:146`),
  `PermissionFactory.CreateCrud` (`:106`),
  `LyricsFactory.CreatePaidWithPromotion` (`:176`).
- **The builder members that express no state any test needs** and duplicate a
  neighbouring method. These are only identifiable after change 1, because before it
  every builder member is trivially unreferenced.
- **Byte-identical aliases**, starting with the clearest case:

```csharp
// tests/Fixtures/Factories/Content/ArticleFactory.cs:12-20 — before
/// <summary>
/// Creates a free article in Draft status with the given category.
/// </summary>
public static ArticleEntity Create(Guid categoryId) => new ArticleBuilder(categoryId).Build();

/// <summary>
/// Creates a free article in Draft status.
/// </summary>
public static ArticleEntity CreateFree(Guid categoryId) => new ArticleBuilder(categoryId).Build();
```

```csharp
// after
/// <summary>
/// Creates a free article in Draft status — the builder default, which is what the
/// overwhelming majority of article tests arrange.
/// </summary>
/// <param name="categoryId">The category the article belongs to.</param>
public static ArticleEntity Create(Guid categoryId) => new ArticleBuilder(categoryId).Build();
```

The two expressions are identical. `ArticleFactory.Create(` has 165 call sites;
`ArticleFactory.CreateFree` has zero. "Free" reads as a meaningful distinction at a
call site, but the builder default already is free, so the name documents nothing.

**Re-measure before deleting.** The counts above are from the audit, taken before
changes 1 and 4. Change 4 creates new call sites for builder methods that are
currently unreferenced, so run the measurement immediately before the deletion
commit, not from this document.

One case needs naming rather than deleting. `AuthDataBuilder`
(`tests/Fixtures/Builders/AuthDataBuilder.cs:13`) is already `public` and is never
named from `tests/Unit` or `tests/Integration`; it is reached only through three
one-line aliases in `AuthTestHelpers` (`:75`, `:81`, `:87`), which between them have
22 call sites. Its own fluent methods `WithUser`, `WithUserPermissions` and
`WithUserPermission` (`:42`, `:53`, `:64`) have two references in the whole suite.
This is the factory-explosion shape in miniature: a builder nobody composes, wrapped
in aliases that hide it. Either delete the unused fluent methods and keep the three
aliases as the intended API, or delete the aliases and let the 22 call sites name
the builder. Do not leave both surfaces.

*If done wrong:* deleting a factory method whose only caller is another factory
method breaks the build in a way that looks like an unrelated failure. Search for
unqualified references inside `tests/Fixtures` as well as qualified `Factory.Method`
references from the test projects.

### 3. Record the layering rule in the fixtures project

**Files:** every class under `tests/Fixtures/Factories/`, plus the builder doc
comments written in change 1.

The rule goes where an author is already looking, which is the type they are about
to call — not a separate document. A markdown file in the fixtures folder would be
read once; a doc comment on `VideoFactory` is read every time someone opens it.

The rule:

| Layer | When it applies |
| --- | --- |
| **Builder** | Any shape a test needs. This is the default and needs no justification. |
| **Factory** | A shape three or more tests share, verbatim. The factory is a named alias for a chain, nothing more. |
| **Inline construction** | Only for the type under test itself, in that type's own test file. |

Applied to `VideoFactory`, whose 25 methods record the chains they replace, most
call sites move to the chain:

```csharp
// before — the factory must own every combination
VideoEntity video = VideoFactory.CreateApprovedWithYoutubeUrl(categoryId);
VideoEntity promoted = VideoFactory.CreatePublishedForArtist(categoryId, artistId);

// after — the call site owns its own combination
VideoEntity video = new VideoBuilder(categoryId).AsApproved().WithYoutubeUrl().Build();
VideoEntity promoted = new VideoBuilder(categoryId).AsPublished().WithArtist(artistId).Build();
```

The second form is longer by a few characters and shorter by one indirection: the
reader learns the arranged state from the test rather than from a factory file in
another folder.

Write the rule as a file-level doc comment on each factory class:

```csharp
/// <summary>
/// Named aliases for <see cref="Builders.Entities.Content.VideoBuilder" /> chains
/// that three or more tests share verbatim. A shape used by fewer than three tests
/// belongs at the call site as a builder chain, not here — factory names carry the
/// combinatorics, and combinatorics multiply.
/// </summary>
public static class VideoFactory
```

The corresponding deletion rule, which runs as part of any fixture change: a public
fixture member with no call site is deleted in the same pull request that made it
unreferenced.

*If done wrong:* stating the rule without applying it to at least one factory leaves
a document nobody believes. `VideoFactory` and `ArticleFactory` are the two largest;
bring both under the rule in this spec so the pattern has precedent.

### 4. Replace reflection writes with builder methods

**Files:** `tests/Fixtures/Factories/Content/VideoFactory.cs`,
`tests/Fixtures/Factories/Content/LyricsFactory.cs`, and the 32 files in
`tests/Unit` holding 170 reflection property writes.

The distribution matters, because the same line means opposite things in the two
places. `tests/Fixtures` holds 9 reflection writes, all reconstituting state the
persistence layer would materialise; seven use `nameof()` and are correct as they
stand. `tests/Integration` holds zero, which is the right number for a suite driving
real entry points, and must stay zero. `tests/Unit` holds 170 across 32 files, and
127 of the suite's 129 bare-string `GetProperty` calls.

**Step 4a — `nameof` at the two unsafe fixture sites.**

```csharp
// tests/Fixtures/Factories/Content/VideoFactory.cs:130-137 — before
public static VideoEntity CreateWithCategory(Guid categoryId, CategoryEntity category)
{
    VideoEntity entity = Create(categoryId);
    typeof(VideoEntity)
        .GetProperty("Category", BindingFlags.Public | BindingFlags.Instance)!
        .SetValue(entity, category);
    return entity;
}
```

```csharp
// after
public static VideoEntity CreateWithCategory(Guid categoryId, CategoryEntity category)
{
    VideoEntity entity = Create(categoryId);

    PropertyInfo navigation = typeof(VideoEntity).GetProperty(
        nameof(VideoEntity.Category),
        BindingFlags.Public | BindingFlags.Instance
    )!;

    navigation.SetValue(entity, category);
    return entity;
}
```

`tests/Fixtures/Factories/Content/LyricsFactory.cs:112-117` takes the identical
change with `nameof(LyricsEntity.Video)`. Rename `VideoEntity.Category` today and
both sites compile, `GetProperty` returns `null`, the `!` suppresses the only
signal, and a test throws a `NullReferenceException` at a line that gives no hint the
cause is a rename three files away. After this step `tests/Fixtures` contains no
bare-string reflection.

**Step 4b — move the `tests/Unit` writes onto builders.** The clearest case reflects
into `IsActive` and `IsVerified` thirteen times in one file:

```csharp
// tests/Unit/Modules/Identity/Application/Shared/Specifications/UserStatusSpecificationsTests.cs:16-29 — before
[Fact]
public void UserIsActiveSpecification_WithActiveUser_ShouldReturnTrue()
{
    // Arrange
    UserEntity user = UserFactory.Create();
    user.GetType().GetProperty("IsActive")!.SetValue(user, true);
    UserIsActiveSpecification spec = new();

    // Act
    bool result = spec.IsSatisfiedBy(user);

    // Assert
    result.Should().BeTrue();
}
```

```csharp
// after
[Theory]
[InlineData(true, true)]
[InlineData(false, false)]
public void UserIsActiveSpecification_ShouldMatchTheUsersActiveState(bool isActive, bool expected)
{
    UserEntity user = isActive ? UserFactory.CreateVerifiedActive() : UserFactory.CreateInactive();

    new UserIsActiveSpecification().IsSatisfiedBy(user).Should().Be(expected);
}
```

`UserBuilder` already exposes `AsVerified()`, `AsUnverified()`, `AsActive()` and
`AsInactive()` at lines 116, 126, 136 and 146, and `UserFactory` already exposes
`CreateVerifiedActive()`, `CreateUnverified()` and `CreateInactive()` at lines 55,
61 and 67. Every one of the thirteen reflection calls in that file has a supported
equivalent that is shorter, type-checked and rename-safe. The same file repeats the
pattern at lines 36, 55, 70, 89, 105, 121, 137, 157, 160, 163, 205, 209, 213 and 217.

`user.GetType().GetProperty(...)` is also the weakest of the three forms in use: it
resolves against the runtime type rather than the declared one, uses a string
literal, and suppresses the null with `!`.

**Where no builder method exists for the state a test needs, add one to the builder**
— do not reflect at the call site. The builder is the single place where a state the
domain cannot express is constructed, so it is the single place a reviewer has to
look to see what fictions the suite relies on. Change 1 is what makes this option
available to test authors for the first time.

**Step 4c — record the rule.** Reflection is acceptable only to reconstitute state
the persistence layer itself would produce: navigation properties populated by
`.Include(...)`, and audit fields stamped by the interceptor. It is never acceptable
to reach a state the domain refuses to enter. If a test needs an entity in a state
the domain will not produce, the question is whether that state is reachable in
production at all — and if it is not, the specification being tested against it is
testing something that cannot happen.

**One false positive to leave alone.** Twelve builders contain lines like
`entity.CreatedAt = _createdAt ?? DateTime.UtcNow;`
(`tests/Fixtures/Builders/Entities/Content/LyricsBuilder.cs:339`). `CreatedAt` is a
public settable property on the shared base class
(`src/Shared/Shared/Domain/Entity.cs:13`), deliberately exposed for the audit
interceptor. That is a plain assignment through a public setter, not reflection and
not a bypass. Do not sweep it up. The `DateTime.UtcNow` on the right-hand side is a
separate finding and belongs to spec 09.

*If done wrong:* mechanically replacing `SetValue` with a builder method that walks
a different path changes what the test arranges. `VideoBuilder.ApplyStatusTransition`
(`VideoBuilder.cs:309-337`) reaches `Published` by calling
`MarkPendingReview() → Approve() → Publish(errors)`, so a test that reflected
`Status = Published` directly and asserted `DomainEvents.Should().BeEmpty()` will now
see the events those transitions raise. That failure is correct — the previous state
was unreachable — but it needs to be read, not silenced.

## Expected fallout

**Change 1 turns nothing red.** Widening visibility cannot break a compile. The
suite should be green immediately after it, which is what makes it a safe first
commit.

**Change 2 turns nothing red if the measurement was correct.** A build failure after
the deletion means a member was called from somewhere the measurement missed —
almost always from inside `tests/Fixtures` itself, unqualified. Restore that member
and re-run the measurement rather than deleting further.

**Change 4b will turn tests red, and each one is a finding.** Two shapes:

1. A test that reflected an entity into a state the domain cannot reach now
   arranges a reachable state and fails, because the specification under test
   behaves differently on a real entity. That is the defect the reflection was
   hiding. Read the specification before changing anything: it may be testing a
   condition that cannot occur.
2. A test that reflected a single field now goes through a transition that writes
   several. Domain-event assertions and `DomainEvents.Should().BeEmpty()` will move.
   Update the assertions to describe the real entity.

**The unit test count will fall.** The `UserStatusSpecificationsTests` collapse alone
turns fourteen facts into a handful of theories. Spec 10 does more of this
deliberately; the reduction here is a side effect of arranging through builders and
is correct.

## Testing

```bash
dotnet build
dotnet test tests/Unit
dotnet test tests/Integration
```

`dotnet build` on its own is the check for changes 1 and 2 and should be run as a
gate between them: change 1 must build green before change 2 deletes anything, or a
deletion failure and a visibility failure become indistinguishable.

The integration suite must stay green throughout. It contains zero reflection writes
and no reference to `Builders.Entities`, so any failure means something outside this
spec's scope moved.

Grep-provable invariants after this spec:

```bash
# every entity builder is public
grep -rn "^internal class" tests/Fixtures/Builders/Entities/        # → nothing

# visibility is expressed per type, not by assembly grant
grep -n "InternalsVisibleTo" tests/Fixtures/_116.Tests.Fixtures.csproj  # → nothing

# no bare-string reflection anywhere in the suite
grep -rn "GetProperty(\"" tests/                                    # → nothing

# reflection lives only in the fixtures layer
grep -rn "SetValue(" tests/Unit tests/Integration                   # → nothing

# the byte-identical alias is gone
grep -rn "CreateFree" tests/                                        # → nothing
```

The new tests that prove the fix are the rewritten specification tests. The mutation
each must catch:

| Mutation to `src/` | Test that must now fail |
| --- | --- |
| make `UserIsActiveSpecification` ignore `IsActive` | `UserIsActiveSpecification_ShouldMatchTheUsersActiveState`, the `false` case |
| rename `VideoEntity.Category` | compile error in `VideoFactory.CreateWithCategory` rather than a runtime null |

Verify the second by performing the rename locally and confirming the build fails at
the fixture, then reverting. That is the entire justification for step 4a and takes
under a minute to demonstrate.

## Risks

**Making builders public invites their use before the layering rule is understood.**
The mitigation is that change 3 lands in the same pull request as change 1, so the
first author to reach a public builder reads the rule in its doc comment. Review is
the enforcement mechanism after that; there is no analyser for it.

**The deletion sweep can remove a member a branch in flight depends on.** Land change
2 as its own commit, immediately before or after a merge window rather than in the
middle of one, and re-measure at the moment of deletion rather than trusting the
audit's number.

**Replacing reflection with builder chains changes what a test arranges, sometimes
correctly and sometimes not.** Take the 32 files one at a time, and for each read the
specification or mapper under test before rewriting the arrangement. A mechanical
sweep across 170 sites will produce arrangements nobody checked, which is the same
failure the reflection caused in the first place.

**`AuthDataBuilder` has two competing surfaces and picking either will annoy
someone.** The 22 call sites read cleanly through the aliases. The recommendation is
to keep the three aliases, delete the three unused fluent methods, and note in
`AuthTestHelpers` that the builder is not part of the public fixture API. Record
whichever choice is made in this spec's implementation notes.

**The `MetaField` question will resurface during the deletion sweep**, because the
Mailer `MetaField` tests look like dead surface to a mechanical scan. They are not in
scope. Exclude those two directories from the sweep explicitly.

## Checklist

- [ ] 1 — all 36 entity builders declared `public`, with doc comments stating the
      layering rule instead of the "prefer the factory" instruction the access
      modifier was enforcing; no `InternalsVisibleTo` added
- [ ] 2 — the dead fixture surface deleted after re-measurement, starting with
      `ArticleFactory.CreateFree` and the 64 uncalled factory methods; the
      `AuthDataBuilder` duplication resolved one way and recorded
- [ ] 3 — the builder / factory / inline rule recorded on every factory class doc
      comment, and applied to `VideoFactory` and `ArticleFactory` in this change
- [ ] 4 — the two bare-string reflection sites use `nameof`; no `SetValue` remains in
      `tests/Unit` or `tests/Integration`; the legitimate persisted-state
      reconstitution stays in `tests/Fixtures`; `entity.CreatedAt = ...` assignments
      left untouched
- [ ] The Mailer `MetaField` init-tests are unchanged, and the decision remains open
