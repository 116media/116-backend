# Medium — Reflection used to build unreachable states

`tests/` contains 179 reflection property writes across 41 files, reached through
180 `GetProperty` and 3 `GetField` calls. They fall into two categories that look
identical in a diff and mean opposite things. Nine of the writes live in
`tests/Fixtures` and reconstitute state the persistence layer would materialise —
legitimate, if two of them are written unsafely. The other 170, across 32 files in
`tests/Unit`, write directly into entities to reach states the domain model does not
offer, bypassing a builder layer that already expresses most of them. `tests/Integration`
contains zero reflection writes, which is correct and should stay that way.

## The problem

### Category 1 — legitimate: reconstituting what EF would materialise

An entity loaded by EF Core with `.Include(...)` has its navigation properties
populated. A test that exercises a mapper reading `entity.Category.Name`, or a
specification reading `entity.Video.CategoryId`, needs an entity in that shape.
The domain constructor cannot produce it, because a navigation property is not
something the domain sets — the persistence layer does. Reflection is the correct
tool here, and the fixtures use it in exactly that role:

```csharp
// tests/Fixtures/Builders/Entities/Content/VideoBuilder.cs:291-296
PropertyInfo publishedProp = typeof(VideoEntity).GetProperty(
    nameof(VideoEntity.PublishedAt),
    BindingFlags.Public | BindingFlags.Instance
)!;

publishedProp.SetValue(entity, _publishedAtOverride);
```

```csharp
// tests/Fixtures/Builders/Entities/Content/CategoryBuilder.cs:202-217
PropertyInfo pinnedProp = typeof(CategoryEntity).GetProperty(
    nameof(CategoryEntity.PinnedToFeedAt),
    BindingFlags.Public | BindingFlags.Instance
)!;

pinnedProp.SetValue(entity, _pinnedToFeedAt);

...

PropertyInfo prop = typeof(CategoryEntity).GetProperty(
    nameof(CategoryEntity.ContentType),
    BindingFlags.Public | BindingFlags.Instance
)!;

prop.SetValue(entity, _contentType);
```

The same shape appears at
`tests/Fixtures/Builders/Entities/Content/ContentOrderItemBuilder.cs:92-97` and
`tests/Fixtures/Builders/Entities/Identity/UserBuilder.cs:217`. All four use
`nameof()`, so a rename in `src/` is a compile error in the fixture rather than a
runtime surprise, and all four are hidden behind a builder method so call sites
never see the reflection.

### Category 2 — unsafe: the same purpose with a bare string literal

Two factories do the same job with the property name as a string and a
null-forgiving operator suppressing the only signal that would catch the mistake:

```csharp
// tests/Fixtures/Factories/Content/VideoFactory.cs:130-137
/// <summary>
/// Creates a free video with the Category navigation property loaded via reflection.
/// Use this when the test exercises a mapper that accesses <c>entity.Category.Name</c>.
/// </summary>
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
// tests/Fixtures/Factories/Content/LyricsFactory.cs:112-117
public static LyricsEntity CreatePublishedWithVideoNavigation(Guid categoryId, VideoEntity video)
{
    LyricsEntity entity = new LyricsBuilder(categoryId).WithVideoId(video.Id).AsPublished().Build();
    typeof(LyricsEntity).GetProperty("Video", BindingFlags.Public | BindingFlags.Instance)!.SetValue(entity, video);
    return entity;
}
```

Rename `VideoEntity.Category` in `src/` and this compiles. `GetProperty` returns
`null`, the `!` tells the compiler not to worry, and the test throws a
`NullReferenceException` at a line that gives no hint the cause is a rename three
files away. These are the only two bare-string reflection sites in `tests/Fixtures`;
the other six use `nameof()`.

### Category 3 — bypass: 170 writes in `tests/Unit` around an existing builder

`tests/Unit` contains 170 reflection property writes across 32 files. Unlike the
fixture cases, these are not reconstituting persisted navigation state — they are
setting domain fields that the builder layer already exposes. The clearest example
is a specification test file that reflects into `IsActive` and `IsVerified` thirteen
times:

```csharp
// tests/Unit/Modules/Identity/Application/Shared/Specifications/UserStatusSpecificationsTests.cs:21
UserEntity user = UserFactory.Create();
user.GetType().GetProperty("IsActive")!.SetValue(user, true);
```

```csharp
// tests/Unit/Modules/Identity/Application/Shared/Specifications/UserStatusSpecificationsTests.cs:36
UserEntity user = UserFactory.Create();
user.GetType().GetProperty("IsActive")!.SetValue(user, false);
```

```csharp
// tests/Unit/Modules/Identity/Application/Shared/Specifications/UserStatusSpecificationsTests.cs:55
UserEntity user = UserFactory.Create();
user.GetType().GetProperty("IsVerified")!.SetValue(user, true);
```

The same file repeats the pattern at lines 70, 89, 105, 121, 137, 157, 160, 163,
205, 209, 213 and 217. Meanwhile `UserBuilder` already exposes `AsActive()`,
`AsInactive()`, `AsVerified()` and `AsUnverified()` (lines 116-146), and
`UserFactory` already exposes `CreateVerifiedActive()`, `CreateInactive()` and
`CreateUnverified()` (lines 55-67). Every one of these thirteen reflection calls has
a supported equivalent that is shorter, type-checked, and rename-safe.

`user.GetType().GetProperty(...)` is also the weakest of the three forms in use: it
resolves against the runtime type rather than the declared one, uses a string
literal, and suppresses the null with `!`.

### The false positive worth stating

Twelve builders under `tests/Fixtures/Builders/` contain lines like:

```csharp
// tests/Fixtures/Builders/Entities/Content/LyricsBuilder.cs:339
entity.CreatedAt = _createdAt ?? DateTime.UtcNow;
```

That looks like the same bypass and is not. `CreatedAt` is a public settable
property on the shared base class:

```csharp
// src/Shared/Shared/Domain/Entity.cs:13
public DateTime? CreatedAt { get; set; }
```

This is a plain assignment through a public setter the domain deliberately exposes
for the audit interceptor to populate. It is correct, requires no change, and should
not be swept up in a mechanical cleanup of the reflection sites.

## Why it matters

Reflection removes the compiler from the loop, and the compiler is the cheapest
reviewer the codebase has.

The immediate cost is fragility. A rename of `VideoEntity.Category` produces a green
build and a `NullReferenceException` inside a test fixture. The developer who did the
rename sees an unrelated test failing on a null dereference and has to work backwards
to the string literal. `nameof()` turns that entire investigation into a red squiggle
at the rename site.

The larger cost is that reflection can construct states the domain forbids. A domain
model exists to make invalid states unrepresentable; `SetValue` represents them
anyway. A test that reflects `IsActive = true` onto a user that the domain would only
activate through a transition is asserting a specification's behaviour against an
object the application can never produce. If the activation path later starts setting
a second field — `ActivatedAt`, or a domain event — the specification may depend on
it and the test will not notice, because the test's user was never activated, only
mutated.

That is the precise inverse of the rule in `CLAUDE.md` about coverage: reflection
used to reach an unreachable state turns green the exact metric that was warning you
the state is unreachable.

There is also a maintenance cost specific to the 170 unit-test sites. They duplicate
the builder layer without benefiting from it: when `UserBuilder.AsActive()` is
updated to reflect a change in what activation means, the thirteen reflection calls in
`UserStatusSpecificationsTests` keep doing whatever they did before.

## The fix

### Step 1 — `nameof()` at the two unsafe fixture sites

```csharp
// Before — tests/Fixtures/Factories/Content/VideoFactory.cs:130-137
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
// After
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

The same change applies to
`tests/Fixtures/Factories/Content/LyricsFactory.cs:112-117`, using
`nameof(LyricsEntity.Video)`. After this, `tests/Fixtures` contains no bare-string
reflection.

### Step 2 — replace the unit-test writes with builder methods

```csharp
// Before — tests/Unit/.../UserStatusSpecificationsTests.cs:15-29
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
// After
[Theory]
[InlineData(true, true)]
[InlineData(false, false)]
public void UserIsActiveSpecification_ShouldMatchTheUsersActiveState(bool isActive, bool expected)
{
    UserEntity user = isActive ? UserFactory.CreateVerifiedActive() : UserFactory.CreateInactive();

    new UserIsActiveSpecification().IsSatisfiedBy(user).Should().Be(expected);
}
```

Two reflection calls and two facts become one theory over the builder's supported
states, and the test now exercises a user the application could actually produce.
This is also the collapse described in
[05-duplication-and-theories.md](05-duplication-and-theories.md).

Where no builder method exists for the state a test needs, the fix is to add one to
the builder — not to reflect at the call site. The builder is the single place where
a state the domain cannot express is constructed, so it is the single place a
reviewer has to look to see what fictions the suite relies on.

### Step 3 — record the rule

Reflection is acceptable only for reconstituting state the persistence layer itself
would produce: navigation properties populated by `.Include(...)`, and audit fields
stamped by the interceptor. It is never acceptable for reaching a state the domain
refuses to enter. If a test needs a user in a state the domain will not produce, the
question is whether the state is reachable in production at all — and if it is not,
the specification being tested against it is testing something that cannot happen.

## The principle

**Reflection in a test is a claim about the persistence layer, not a shortcut around
the domain.** The legitimate use is narrow and identifiable: it puts an entity into
the shape EF Core would have handed the code under test. Every other use is the test
asserting behaviour against an object the application cannot construct.

Three rules follow:

1. **Every reflected member name is a `nameof()`.** A string literal plus `!` makes a
   rename a runtime failure in an unrelated file. There is no case where the literal
   is preferable.
2. **Reflection lives in `tests/Fixtures`, behind a builder or factory method.** A
   test body containing `GetProperty(...).SetValue(...)` is misplaced; the state it
   needs belongs on the builder, where every test can reach it and one reviewer can
   audit it.
3. **Never reflect to bypass a domain invariant.** If the domain will not let you
   into a state, that is information. Encode the state through the transition that
   produces it, or establish that the state is unreachable and delete the test.

## Checklist

- [ ] No `GetProperty` / `GetField` call in the suite uses a string literal;
      all use `nameof()`.
- [ ] No reflection appears in a test body — only in `tests/Fixtures` builders and
      factories.
- [ ] `tests/Integration` contains zero reflection writes.
- [ ] Before reflecting into a property, the builder for that entity was checked for
      an existing method expressing the state.
- [ ] The reflected property is one the persistence layer populates (navigation or
      audit field), not one guarded by a domain transition.
- [ ] Plain assignments through public setters, such as `entity.CreatedAt = ...`, are
      left alone — they are not reflection and not a bypass.
