# Critical — Assertions that cannot fail

Three patterns account for several hundred tests whose assertions are true by
construction. They inflate the test count and the coverage percentage while
proving nothing, and — worse — they mark their subject as "covered" so nobody
writes the test that would work.

## Pattern 1 — Localization tests compare a value against itself

104 test files assert a localized message against **the same localizer instance
the code under test uses**.

```csharp
// tests/Unit/Modules/Identity/Application/Auth/UseCases/Admin/Commands/Login/AdminLoginValidatorTests.cs:212-230
[Theory]
[InlineData("en")]
[InlineData("fr")]
public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
{
    Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
    Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
    var i18n = TestErrorsFactory.CreateIdentityI18n();
    var validator = new AdminLoginValidator(i18n);          // validator uses THIS i18n
    ...
    result.ShouldHaveValidationErrorFor(x => x.Email)
          .WithErrorMessage(i18n.User.Validation.EmailRequired());  // expected from THE SAME i18n
}
```

Both sides resolve through the identical `IStringLocalizer` under the identical
culture. Expected equals actual by construction, in every culture, always.

**What this means concretely:** if `ValidationErrorMessage.fr.resx` were emptied
tomorrow, `IStringLocalizer` would fall back to the neutral string on *both* sides
and all 208 test executions would still pass. The project has 99 `.resx` files and
208 test executions claiming to guard their translations. They guard nothing.

### Fix — pin the expected string

Pin the expected string as a literal, so the resource file is actually consulted:

```csharp
[Theory]
[InlineData("en", "Email is required.")]
[InlineData("fr", "L'adresse e-mail est requise.")]
public async Task Validate_WithMissingEmail_ShouldReturnMessageInRequestCulture(
    string culture,
    string expected
)
{
    using var _ = new CultureScope(culture);
    var validator = new AdminLoginValidator(TestErrorsFactory.CreateIdentityI18n());

    TestValidationResult<AdminLoginCommand> result = await validator.TestValidateAsync(
        new AdminLoginCommand(Email: null!, Password: TestConstants.User.ValidPassword)
    );

    result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(expected);
}
```

Better still, replace all 104 with **one** resource-completeness theory that
asserts every key present in the neutral `.resx` exists in `en` and `fr` with a
non-empty, distinct value. That single test catches every missing translation
across all 99 resource files — which the current 104 files catch none of.

## Pattern 2 — Asserting a hard-coded return value

```csharp
// src/.../AdminPublishArticleHandler.cs:52
return new AdminPublishArticleResult(IsSuccess: true);   // unconditional
```

```csharp
// tests/Unit/.../AdminPublishArticleHandlerTests.cs:52
result.IsSuccess.Should().BeTrue();                      // cannot be false
```

`IsSuccess.Should().BeTrue()` appears **141 times across 93 files**, against **99
result types in `src/` that hard-code `IsSuccess: true`**. The handler either
throws or returns success; the boolean carries no information, so asserting it
carries none either.

This is not harmful on its own — it is harmful because in 24 of the 35 handlers
that invoke a domain transition, it is the *only* outcome assertion in the test.
See [02-state-transition-blindness.md](02-state-transition-blindness.md).

## Pattern 3 — `BeOfType<T>` on a variable already typed `T`

```csharp
// tests/Unit/Modules/Core/Application/Shared/Errors/CoreErrorsTests.cs:23-25
BadRequestException exception = _errors.SomeError();
exception.Should().BeOfType<BadRequestException>();   // the compiler already proved this
```

112 of 125 `BeOfType<T>()` assertions are compiler-guaranteed. The runtime check
can only fail for a subtype, and none of these factories return one.

### Fix — assert the real predicate

Declare the variable as the base type, which makes the assertion real:

```csharp
BaseException exception = _errors.RoleAlreadyActive();
exception.Should().BeOfType<ConflictException>();     // now a genuine check
```

## Pattern 4 — `Should().NotBeNull()` on something that cannot be null

234 tests assert non-nullability of a value the runtime guarantees.

```csharp
// tests/Unit/Modules/Content/Infrastructure/Persistence/ContentDbContextTests.cs:22-28
using var context = new ContentDbContext(CreateOptions());
DbSet<ContentTypeEntity> result = context.ContentTypes;
result.Should().NotBeNull();          // EF initialises every DbSet property
```

57 such tests exist across three DbContext test files, each spinning up a fresh
in-memory provider to assert something EF Core guarantees.

The most misleading variant is in the specification tests:

```csharp
// tests/Unit/Modules/Content/Application/Editorial/Specifications/ArticleSpecificationsTests.cs:52-63
Func<ArticleEntity, bool> predicate = spec.ToExpression().Compile();
predicate.Should().NotBeNull();       // Compile() never returns null
```

14 such tests across 8 specification files. The specification's actual predicate
semantics have never been evaluated, but the file's existence reports the
specification as covered.

### Fix — assert a mistake could break it

Assert the behaviour the type exists to provide:

```csharp
[Theory]
[InlineData("fally-ipupa-portrait", true)]
[InlineData("FALLY-IPUPA-PORTRAIT", true)]
[InlineData("koffi-olomide", false)]
public void ArticleBySlugSpecification_ShouldMatchSlugCaseInsensitively(string slug, bool expected)
{
    ArticleEntity article = ArticleFactory.CreateWithSlug(CategoryId, "fally-ipupa-portrait");

    new ArticleBySlugSpecification(slug).IsSatisfiedBy(article).Should().Be(expected);
}
```

For the DbContext case, assert something a mistake could break — that every domain
entity is actually mapped with a primary key:

```csharp
public static TheoryData<Type> MappedEntities() =>
    new(typeof(ArticleEntity), typeof(VideoEntity), typeof(LyricsEntity) /* ... */);

[Theory]
[MemberData(nameof(MappedEntities))]
public void Model_ShouldMapEntityWithPrimaryKey(Type entityType)
{
    using var context = new ContentDbContext(CreateOptions());

    IEntityType? mapped = context.Model.FindEntityType(entityType);

    mapped.Should().NotBeNull();
    mapped!.FindPrimaryKey().Should().NotBeNull();
}
```

That collapses 49 tests into one and catches an entity added to the domain but
never mapped — which the 49 could not.

## The principle

Before writing an assertion, ask: **what change to `src/` would make this fail?**
If the honest answer is "none", or "only a change no one would make", the assertion
is decoration.

Three specific rules follow:

1. **Never derive the expected value from the system under test.** Expected values
   are literals, or come from an independent source. The moment the test asks the
   same object the code asks, it is comparing a value to itself.
2. **Do not assert what the compiler or framework guarantees.** Non-nullability of
   a non-nullable, the type of a variable already declared as that type, that
   `Compile()` returned something.
3. **Assert the outcome, not the return envelope.** A hard-coded `IsSuccess: true`
   tells you the method reached its last line — which the absence of an exception
   already told you.

## Scale and priority

| Pattern | Occurrences | Files |
| --- | --- | --- |
| Localization self-comparison | 208 executions | 104 |
| `IsSuccess.Should().BeTrue()` on a constant | 141 | 93 |
| `Should().NotBeNull()` on a non-nullable | 234 | ~40 |
| `BeOfType<T>` on a `T` variable | 112 | ~26 |

The localization cluster is the highest priority: it is the only one that claims
to protect a real, easily-broken asset (99 resource files across two languages)
and delivers zero protection.
