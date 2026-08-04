# Medium-High — Fact/Theory duplication hides coverage gaps

The suite contains 8,279 `[Fact]` methods and 298 `[Theory]` methods, a ratio of 28
to 1, and uses `[MemberData]` three times across 878 unit test files. Structural
analysis of the fact bodies shows 1,643 of them (26%) sit in 572 clusters whose
members differ by at most a single token after literals are normalised. Collapsing
those clusters would replace 1,643 facts with 572 theories. The cost of the
duplication is not verbosity — it is that a duplicated block is written by copying,
and a copy is never audited for the case it forgot.

## The problem

### The measured shape

| Metric | Value |
| --- | --- |
| `[Fact]` methods | 8,279 |
| `[Theory]` methods | 298 |
| `[MemberData]` usages, unit suite | 3 |
| Facts scanned for structural duplication | 6,409 |
| Facts in a near-identical cluster | 1,643 (26%) |
| Distinct clusters | 572 |
| Facts remaining after collapse | ~5,338 |

### The worst clusters

**`tests/Unit/Modules/Content/Infrastructure/Persistence/ContentDbContextTests.cs`** —
50 facts, of which 47 are the same three lines with two identifiers changed:

```csharp
[Fact]
public void ContentTypes_ShouldReturnDbSet()
{
    using var context = new ContentDbContext(CreateOptions());
    DbSet<ContentTypeEntity> result = context.ContentTypes;
    result.Should().NotBeNull();
}

[Fact]
public void PricingTiers_ShouldReturnDbSet()
{
    using var context = new ContentDbContext(CreateOptions());
    DbSet<PricingTierEntity> result = context.PricingTiers;
    result.Should().NotBeNull();
}
```

Forty-seven times, each spinning up a fresh in-memory provider to assert something
EF Core guarantees.

**`tests/Unit/Modules/Identity/Application/Shared/Errors/UserErrorsTests.cs`** —
51 facts, 46 of which follow one of four message-factory shapes. Seventeen are the
conflict-exception shape verbatim:

```csharp
[Fact]
public void EmailAlreadyExists_ShouldReturnConflictException()
{
    // Arrange
    const string email = "user@example.com";

    // Act
    ConflictException exception = _errors.EmailAlreadyExists(email);

    // Assert
    exception.Should().BeOfType<ConflictException>();
    exception.Message.Should().Be(_conflict.EmailAlreadyExists(email));
}

[Fact]
public void UsernameAlreadyExists_ShouldReturnConflictException()
{
    // Arrange
    const string username = "john_doe";

    // Act
    ConflictException exception = _errors.UsernameAlreadyExists(username);

    // Assert
    exception.Should().BeOfType<ConflictException>();
    exception.Message.Should().Be(_conflict.UsernameAlreadyExists(username));
}
```

**`tests/Unit/Shared/Exceptions/Handlers/Strategies/`** — 13 files, 117 facts. The
files differ only in which exception type and status code they exercise, so the
duplication is cross-file and invisible to anyone reading a single file.

**`tests/Unit/Modules/Identity/Application/Auth/Validators/CredentialValidationTests.cs`** —
47 facts, 26 in one-token clusters.

**`tests/Unit/Modules/Core/Infrastructure/Services/CloudinaryServiceTests.cs`** —
38 facts, 32 in one-token clusters, the largest single-file cluster in the suite.

### The proof that duplication hides gaps

`src/Modules/Content/Content/Infrastructure/Cache/` contains three invalidators:

```
CacheInvalidator.cs
PopularArticlesCacheInvalidator.cs
PopularTagsCacheInvalidator.cs
PopularVideosCacheInvalidator.cs
```

`tests/Unit/Modules/Content/Infrastructure/Cache/` contains two test files, and they
are *identical*. Normalising `Article`/`article` in one file and `Tag`/`tag` in the
other produces byte-identical output — `diff` reports no differences at all between
`PopularArticlesCacheInvalidatorTests.cs` and `PopularTagsCacheInvalidatorTests.cs`.

`PopularVideosCacheInvalidator` has no test file. A mock for it exists
(`tests/Unit/Common/Mocks/Infrastructure/MockPopularVideosCacheInvalidator.cs`), so
other tests are aware of the type; nobody copied the file a third time.

That is the failure mode in one page. The second test file was produced by copying
the first and running a find-and-replace. The third was not produced, because
producing it required someone to remember. A `[Theory]` enumerating the
invalidator types out of the assembly would have covered all three the moment the
third was written, without anyone remembering anything.

## Why it matters

Copy-paste tests fail in a specific direction: they cover exactly the cases someone
thought to copy, and silently omit everything else. Because the omission looks like
absence rather than like a failure, it is never surfaced by the run report. The
`PopularVideosCacheInvalidator` gap has the same signature as a deliberate decision
not to test it.

The second cost is that duplication makes coverage numbers unreliable in both
directions. 47 near-identical `DbSet` facts inflate the unit test count by 46 and
the reported line coverage of `ContentDbContext` to near 100%, while the model
configuration those DbSets depend on is not exercised at all. Anyone using the
coverage number to decide where to invest is steered away from the file that most
needs work.

The third cost is maintenance drag. When `UserErrors` gains a fifth error category,
the correct change is one new `TheoryData` row. In the current shape it is a new
`[Fact]` copied from a neighbouring one, and the reviewer has to diff two near-
identical 12-line blocks to see whether the copy was adapted correctly.

## The fix

### Collapse the `UserErrors` conflict cluster

```csharp
// Before — tests/Unit/Modules/Identity/Application/Shared/Errors/UserErrorsTests.cs
// (17 facts of this shape)
[Fact]
public void EmailAlreadyExists_ShouldReturnConflictException()
{
    const string email = "user@example.com";

    ConflictException exception = _errors.EmailAlreadyExists(email);

    exception.Should().BeOfType<ConflictException>();
    exception.Message.Should().Be(_conflict.EmailAlreadyExists(email));
}

[Fact]
public void UsernameAlreadyExists_ShouldReturnConflictException()
{
    const string username = "john_doe";

    ConflictException exception = _errors.UsernameAlreadyExists(username);

    exception.Should().BeOfType<ConflictException>();
    exception.Message.Should().Be(_conflict.UsernameAlreadyExists(username));
}
```

```csharp
// After — one theory over the (factory, expected message) pairs
public static TheoryData<string, Func<UserErrors, string, BaseException>, Func<ConflictErrorMessage, string, string>>
    ConflictFactories() =>
    new()
    {
        { "user@example.com", (e, v) => e.EmailAlreadyExists(v), (m, v) => m.EmailAlreadyExists(v) },
        { "john_doe", (e, v) => e.UsernameAlreadyExists(v), (m, v) => m.UsernameAlreadyExists(v) },
        { "+1234567890", (e, v) => e.PhoneNumberAlreadyExists(v), (m, v) => m.PhoneNumberAlreadyExists(v) },
    };

[Theory]
[MemberData(nameof(ConflictFactories))]
public void ConflictFactory_ShouldReturnConflictExceptionWithLocalizedMessage(
    string value,
    Func<UserErrors, string, BaseException> factory,
    Func<ConflictErrorMessage, string, string> expected
)
{
    BaseException exception = factory(_errors, value);

    exception.Should().BeOfType<ConflictException>();
    exception.Message.Should().Be(expected(_conflict, value));
}
```

Note the declared type of `exception` is `BaseException`, not `ConflictException`.
That makes the `BeOfType<ConflictException>()` assertion a real runtime check rather
than a restatement of the compiler's knowledge — the fix from
[01-assertions-that-cannot-fail.md](01-assertions-that-cannot-fail.md), applied
once instead of seventeen times.

### Collapse `ContentDbContextTests` into a theory that can find a gap

The 47 `DbSet` facts should not become a theory over 47 property names — that would
preserve the copy-paste in a different syntax and still assert nothing. Make the
theory range over the domain's entity types and assert the mapping, so an entity
added to `Domain/Entities/` and never configured fails the suite:

```csharp
// Before — 47 facts of this shape
[Fact]
public void ContentTypes_ShouldReturnDbSet()
{
    using var context = new ContentDbContext(CreateOptions());
    DbSet<ContentTypeEntity> result = context.ContentTypes;
    result.Should().NotBeNull();
}
```

```csharp
// After
public static TheoryData<Type> DomainEntities() =>
    new(
        typeof(ContentTypeEntity).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.Name.EndsWith("Entity"))
            .OrderBy(t => t.Name)
    );

[Theory]
[MemberData(nameof(DomainEntities))]
public void Model_ShouldMapEveryDomainEntityWithAPrimaryKey(Type entityType)
{
    using var context = new ContentDbContext(CreateOptions());

    IEntityType? mapped = context.Model.FindEntityType(entityType);

    mapped.Should().NotBeNull($"{entityType.Name} is a domain entity and must be mapped");
    mapped!.FindPrimaryKey().Should().NotBeNull();
}
```

Because the theory data comes from the assembly rather than from a hand-written
list, a new entity is covered the moment it is written. That is the property the 47
facts lack and cannot be given.

### Apply the same shape to the invalidators

```csharp
public static TheoryData<Type> CacheInvalidators() =>
    new(
        typeof(CacheInvalidator).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ICacheInvalidator).IsAssignableFrom(t))
    );

[Theory]
[MemberData(nameof(CacheInvalidators))]
public async Task Invalidator_ShouldRemoveItsKeyPrefixFromCache(Type invalidatorType)
{
    // Resolve, invoke, and assert the cache remove call for whichever invalidators exist.
}
```

`PopularVideosCacheInvalidator` is covered without anyone noticing it was missing.

### The habit already exists

Two files show the target state and should be cited in review rather than
rewritten:

- `tests/Unit/Modules/Identity/Application/Session/UseCases/Admin/Queries/ExportSessionData/AdminExportSessionDataValidatorTests.cs`
  uses 9 theories carrying 32 `[InlineData]` rows alongside 8 facts — parameterised
  where the cases vary by value, facts where they do not.
- `tests/Unit/Modules/Mailer/Application/Templates/EmailTemplateRendererTests.cs:70-91`
  builds its `TheoryData` as the cross product of every `EnumEmailTemplate` value
  and both cultures, then asserts no placeholder survives rendering. A new template
  added to the enum is covered automatically:

```csharp
// tests/Unit/Modules/Mailer/Application/Templates/EmailTemplateRendererTests.cs:70-91
var data = new TheoryData<EnumEmailTemplate, string>();

foreach (EnumEmailTemplate template in Enum.GetValues<EnumEmailTemplate>())
{
    data.Add(template, "en");
    data.Add(template, "fr");
}

return data;

[Theory]
[MemberData(nameof(AllTemplateCultures))]
public void Render_EveryTemplateInEveryCulture_ShouldLeaveNoPlaceholder(EnumEmailTemplate template, string culture)
{
    RenderedEmail rendered = Renderer.Render(template, AllTokens, culture);

    rendered.Subject.Should().NotContain("{{").And.NotBeNullOrWhiteSpace();
    rendered.HtmlBody.Should().NotContain("{{");
    rendered.TextBody.Should().NotContain("{{").And.NotBeNullOrWhiteSpace();
}
```

The skill is present in the codebase. It is simply not the default reaction when a
new case needs covering.

## The principle

**If the second test is the first test with a value changed, it is a row, not a
method.** The question that decides between `[Fact]` and `[Theory]` is not how many
cases there are; it is whether the cases differ in *structure* or only in *data*.

Two rules follow:

1. **Prefer theory data derived from the type system over hand-written rows.**
   `Enum.GetValues<T>()`, `assembly.GetTypes().Where(...)`, and interface scans give
   a theory the property a list of `[InlineData]` cannot have: it grows when the
   production code grows. A hand-written row list is copy-paste with better
   formatting.
2. **Treat a copy-paste test file as a signal to check for a sibling.** Two files
   identical modulo a type name almost always means there is a third type that got
   neither. Before writing the second copy, write the theory.

## Checklist

- [ ] Before adding a `[Fact]`, check whether an existing fact in the file differs
      only in literal values. If so, convert both into a `[Theory]`.
- [ ] Theory data that enumerates types, enum members, or registered
      implementations is sourced from the assembly or the enum, not hand-listed.
- [ ] When a test file is created by copying another and renaming a type, every
      other implementation of that abstraction is checked for a test file.
- [ ] A theory replacing N facts asserts at least as much as the N facts did, not
      the intersection of what they had in common.
- [ ] `[MemberData]` factories are `public static` and return `TheoryData<...>`, so
      the parameter types are checked at compile time.
