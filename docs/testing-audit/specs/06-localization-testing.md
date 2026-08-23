# Spec 06 — Localization testing

## Goal

104 test files claim to guard the project's translations and guard none of them.
Each asks the same `IStringLocalizer` the code under test uses for the expected
string, so expected equals actual by construction in every culture. This spec
deletes those 104 files and replaces them with one resource-completeness theory
that reads the compiled `.resx` sets directly and asserts that every key in the
neutral catalogue exists in `en` and `fr`, is non-empty, carries the same format
placeholders, and — for `fr` — is actually translated. It also repairs
`LocalizerFactory` and `TestLocalizer`, whose `culture` parameter has never had an
effect, and keeps the small number of end-to-end tests that assert real translated
strings over HTTP.

## Scope

In scope:

- One new resource-completeness theory covering all 99 `.resx` files across 33
  resource families in five assemblies.
- Deletion of the 104 `*_ShouldBeLocalizedForCulture` tests.
- `tests/Fixtures/Helpers/LocalizerFactory.cs` and
  `tests/Fixtures/Helpers/TestLocalizer.cs`: one cached container, no `culture`
  parameter, doc comments that state where the culture actually comes from.
- `tests/Fixtures/Helpers/TestErrorsFactory.cs:257-263`, the one call site that
  passes `"en"` and documents a guarantee it does not get.
- The handful of genuine end-to-end localization tests, kept and extended.

Not in this spec:

- Extending `CultureScope` to cover `CurrentCulture` as well as `CurrentUICulture`,
  and joining the culture-mutating tests to a non-parallel collection. That is spec
  02, and this spec depends on it.
- Request-localization middleware wiring in `src/`. It works for an explicit
  `Accept-Language` header; the `PublicLoginEndpointV1Tests` French case proves it.
  What no test proves is the header-*absent* default, which is `fr` — change 4 adds
  that assertion rather than changing the wiring.
- Adding translations. This spec adds the test that finds missing ones. Whatever it
  finds is filed as its own work item.
- The other assertion families in the same files. If a deleted file's *other* tests
  also carry `BeOfType` or `NotBeNull` defects, those are spec 05's; delete only the
  culture theory and leave the rest of the file for spec 05.

## Prerequisites

- **Spec 02 (test isolation)** must land first. It extends `CultureScope` to save
  and restore `CurrentCulture` alongside `CurrentUICulture`. Change 3 below tells
  callers to pin culture by wrapping the assertion in a `CultureScope`, which is
  only correct once the scope covers both.
- **Spec 01** indirectly, for change 4: the end-to-end localization tests assert
  against the real request-localization pipeline, and that is only trustworthy once
  the test host matches production.

## Decision recorded

The index offers two options and recommends the first. **The recorded decision is
to replace, not to fix in place.**

*Replace.* One theory covers all 99 resource files and every key in them. It fails
when a translator adds a key to the neutral catalogue and forgets `fr`, which is
the failure that actually happens. It costs one file.

*Fix in place (the alternative, recorded and not chosen).* Pin the expected string
as a literal in each of the 104 files:

```csharp
[Theory]
[InlineData("en", "Email is required.")]
[InlineData("fr", "L'adresse e-mail est requise.")]
public async Task Validate_WithMissingEmail_ShouldReturnMessageInRequestCulture(
    string culture,
    string expected
)
{
    var validator = new AdminLoginValidator(TestErrorsFactory.CreateIdentityI18n());

    using var _ = new CultureScope(culture);

    TestValidationResult<AdminLoginCommand> result = await validator.TestValidateAsync(
        new AdminLoginCommand(Email: null!, Password: TestConstants.User.ValidPassword)
    );

    result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(expected);
}
```

This is a correct test and would be the right form for a *new* test of a specific
message. It was rejected as the bulk remediation for two reasons. It costs 104 file
edits and roughly 400 hand-copied literals, each of which is a place for a typo that
turns into a false failure. And when it is finished it covers only the strings those
104 files happen to name — a few hundred keys of the several thousand in the
catalogue — while the theory covers all of them. If a specific message's exact
wording is contractual, write the pinned-literal form for that message; do not write
it 104 times.

## Changes

### 1. Add the resource-completeness theory

**File (new):** `tests/Unit/Shared/Localization/ResourceCompletenessTests.cs`

The 33 resource families live in five assemblies:

| Assembly | Resource directory | Families |
| --- | --- | --- |
| `Shared` | `src/Shared/Shared/Application/Exceptions/Messages` | `SharedExceptionMessage` |
| `Core` | `src/Modules/Core/Core/Application/Shared/Errors/Messages` | `InternalServerErrorMessage`, `ValidationErrorMessage` |
| `Identity` | `src/Modules/Identity/Identity/Application/Shared/Errors/Messages` | `AuthenticationErrorMessage`, `AuthorizationErrorMessage`, `ConflictErrorMessage`, `ValidationErrorMessage`, and the rest |
| `Content` | `src/Modules/Content/Content/Application/Shared/Errors/Messages` | `ArticleErrorMessage`, `VideoErrorMessage`, `LyricsErrorMessage`, and 15 more |
| `Mailer` | `.../Application/Shared/Errors/Messages`, `.../Application/Notifications/Messages`, `.../Application/Templates/Messages` | `NewsletterErrorMessage`, `NotificationErrorMessage`, `NotificationMessage`, `EmailTemplateMessage` |

Two families share the base name `ValidationErrorMessage` in different assemblies,
so the discovery key must be assembly-qualified, never the short name.

**Discovering the families programmatically.** A neutral `.resx` compiles into an
embedded resource in its own assembly; the `.en.resx` and `.fr.resx` compile into
satellite assemblies. So the discovery pass enumerates the *neutral* set from the
main assembly's manifest, and the culture sets are reached through `ResourceManager`,
which loads the satellites:

```csharp
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using _116.Content;
using _116.Core;
using _116.Identity;
using _116.Mailer;
using _116.Shared.Application.Exceptions.Messages;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Localization;

/// <summary>
/// Asserts that every key defined in a neutral message catalogue is present and
/// populated in the English and French satellites. This replaces the 104 per-file
/// culture tests, each of which asked the system's own localizer for the expected
/// string and therefore could not fail.
/// </summary>
public class ResourceCompletenessTests
{
    /// <summary>
    /// One anchor type per assembly that ships message resources. Adding a module
    /// with its own catalogue means adding its marker type here.
    /// </summary>
    private static readonly Assembly[] ResourceAssemblies =
    [
        typeof(SharedExceptionMessage).Assembly,
        typeof(CoreModule).Assembly,
        typeof(IdentityModule).Assembly,
        typeof(ContentModule).Assembly,
        typeof(MailerModule).Assembly,
    ];

    /// <summary>
    /// Every neutral message catalogue found in the resource assemblies, as
    /// (assembly, resource base name) pairs. Discovery is by manifest inspection
    /// rather than a hand-maintained list, so a catalogue added to <c>src/</c> is
    /// covered without touching this file.
    /// </summary>
    public static TheoryData<string, string> Catalogues()
    {
        TheoryData<string, string> data = new();

        foreach (Assembly assembly in ResourceAssemblies)
        {
            IEnumerable<string> baseNames = assembly
                .GetManifestResourceNames()
                .Where(name => name.EndsWith(".resources", StringComparison.Ordinal))
                .Select(name => name[..^".resources".Length])
                .Where(name =>
                    name.EndsWith("ErrorMessage", StringComparison.Ordinal)
                    || name.EndsWith("TemplateMessage", StringComparison.Ordinal)
                    || name.EndsWith("ExceptionMessage", StringComparison.Ordinal)
                    || name.EndsWith("NotificationMessage", StringComparison.Ordinal)
                );

            foreach (string baseName in baseNames)
            {
                data.Add(assembly.GetName().Name!, baseName);
            }
        }

        return data;
    }
}
```

`Catalogues()` must be asserted non-empty by its own fact, because a discovery
filter that matches nothing produces a theory with zero cases and a green run:

```csharp
    /// <summary>
    /// Guards the discovery itself. A filter that stops matching would silently
    /// reduce this file to zero test cases.
    /// </summary>
    [Fact]
    public void Catalogues_ShouldDiscoverEveryShippedResourceFamily()
    {
        Catalogues().Should().HaveCount(33);
    }
```

The completeness assertion itself, per catalogue and per culture:

```csharp
    [Theory]
    [MemberData(nameof(Catalogues))]
    public void EveryNeutralKey_ShouldBePresentAndPopulatedInEveryCulture(
        string assemblyName,
        string baseName
    )
    {
        Assembly assembly = ResourceAssemblies.Single(a => a.GetName().Name == assemblyName);
        ResourceManager manager = new(baseName, assembly);

        IReadOnlyDictionary<string, string> neutral = ReadSet(manager, CultureInfo.InvariantCulture);
        neutral.Should().NotBeEmpty($"{baseName} defines no keys");

        foreach (string culture in new[] { "en", "fr" })
        {
            IReadOnlyDictionary<string, string> translated = ReadSet(manager, new CultureInfo(culture));

            translated
                .Keys.Should()
                .BeEquivalentTo(
                    neutral.Keys,
                    $"{baseName}.{culture}.resx must define exactly the neutral key set"
                );

            foreach ((string key, string neutralValue) in neutral)
            {
                translated[key]
                    .Should()
                    .NotBeNullOrWhiteSpace($"{baseName}.{culture}.resx['{key}'] is empty");

                Placeholders(translated[key])
                    .Should()
                    .BeEquivalentTo(
                        Placeholders(neutralValue),
                        $"{baseName}.{culture}.resx['{key}'] must format the same arguments as the neutral string"
                    );
            }
        }
    }
```

`BeEquivalentTo` on the key sets catches both halves of the drift: a neutral key
with no translation, and a stale translated key whose neutral entry was deleted.
The placeholder check catches the failure that reaches production as a
`FormatException` — a French string that drops `{0}` while the calling code still
passes an argument, as `ConflictErrorMessage.EmailAlreadyExists` would.

The "distinct where it should differ" assertion applies to `fr` only. The neutral
catalogue is written in English, so `en` legitimately equals it key for key;
requiring `en` to differ would be wrong. French must differ, except for the short
list of strings that are the same in both languages:

```csharp
    /// <summary>
    /// Keys whose French value is legitimately identical to the neutral English
    /// value — proper nouns, format-only strings, and untranslatable tokens.
    /// Every entry needs a reason, and an entry added to silence a failure is a
    /// missing translation wearing a disguise.
    /// </summary>
    private static readonly HashSet<string> IdenticalByDesign = [];

    [Theory]
    [MemberData(nameof(Catalogues))]
    public void FrenchCatalogue_ShouldNotRepeatTheNeutralEnglishString(
        string assemblyName,
        string baseName
    )
    {
        Assembly assembly = ResourceAssemblies.Single(a => a.GetName().Name == assemblyName);
        ResourceManager manager = new(baseName, assembly);

        IReadOnlyDictionary<string, string> neutral = ReadSet(manager, CultureInfo.InvariantCulture);
        IReadOnlyDictionary<string, string> french = ReadSet(manager, new CultureInfo("fr"));

        IEnumerable<string> untranslated = neutral
            .Where(entry =>
                french.TryGetValue(entry.Key, out string? value)
                && value == entry.Value
                && !IdenticalByDesign.Contains($"{baseName}.{entry.Key}")
            )
            .Select(entry => entry.Key);

        untranslated
            .Should()
            .BeEmpty($"{baseName}.fr.resx repeats the neutral English string for these keys");
    }
```

The two helpers, private to the file:

```csharp
    /// <summary>
    /// Reads one culture's resource set without falling back to a parent culture,
    /// so a missing satellite is observable as an empty set rather than silently
    /// resolving to the neutral strings.
    /// </summary>
    private static IReadOnlyDictionary<string, string> ReadSet(ResourceManager manager, CultureInfo culture)
    {
        ResourceSet? set = manager.GetResourceSet(culture, createIfNotExists: true, tryParents: false);

        if (set is null)
        {
            return new Dictionary<string, string>();
        }

        return set.Cast<DictionaryEntry>()
            .Where(entry => entry.Value is string)
            .ToDictionary(entry => (string)entry.Key, entry => (string)entry.Value!);
    }

    /// <summary>
    /// Extracts the composite-format argument indexes used by a message, so that a
    /// translation dropping or inventing a placeholder fails before it reaches a
    /// caller of <see cref="string.Format(string, object?[])" />.
    /// </summary>
    private static IReadOnlyCollection<string> Placeholders(string value) =>
        Regex.Matches(value, @"\{(\d+)(?::[^}]*)?\}").Select(match => match.Groups[1].Value).Distinct().ToList();
```

`tryParents: false` is the load-bearing argument. With the default `true`, a missing
`fr` satellite resolves to the neutral set and the theory passes — which is exactly
the failure mode of the 104 tests being deleted.

*If done wrong:* calling `IStringLocalizer` instead of `ResourceManager` reproduces
the original defect at a larger scale. The whole point is that the expected values
come from an independent reader of the compiled resources, not from the abstraction
the application uses.

### 2. Delete the 104 culture tests

**Files:** the 104 files matched by
`grep -rln "ShouldBeLocalizedForCulture" tests/Unit`.

Each contains a theory of this shape:

```csharp
// tests/Unit/Modules/Identity/Application/Auth/UseCases/Admin/Commands/Login/AdminLoginValidatorTests.cs:212-230 — deleted
[Theory]
[InlineData("en")]
[InlineData("fr")]
public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
{
    Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
    Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
    var i18n = TestErrorsFactory.CreateIdentityI18n();
    var validator = new AdminLoginValidator(i18n);
    ...
    result.ShouldHaveValidationErrorFor(x => x.Email)
          .WithErrorMessage(i18n.User.Validation.EmailRequired());
}
```

Delete the theory method only, not the file. Then remove the now-unused
`using System.Globalization;` directive if nothing else in the file needs it. All
104 files also contain the unrestored culture assignment that spec 02 is fixing, so
coordinate: if spec 02 has already substituted `CultureScope` at these sites,
deleting the method removes the substitution too, which is correct and expected.

The deletion removes 208 test executions from the run report. That is the intended
outcome — every one of them passes against an emptied `.resx`, and the theory added
in change 1 does not.

*If done wrong:* deleting the whole file takes the validator's real rule tests with
it. Delete methods, verify the file still contains its non-localization tests, and
diff the per-file test count before and after.

### 3. Fix `LocalizerFactory` and `TestLocalizer`

**Files:** `tests/Fixtures/Helpers/LocalizerFactory.cs`,
`tests/Fixtures/Helpers/TestLocalizer.cs`,
`tests/Fixtures/Helpers/TestErrorsFactory.cs`.

Both helpers wrap a `CultureScope` around *construction* and return. The scope
disposes as control leaves the method, and `ResourceManagerStringLocalizer` reads
`CultureInfo.CurrentUICulture` in its indexer, at access time. The parameter has
never had an effect. Both also build and abandon a `ServiceProvider` per call, in a
suite where `TestErrorsFactory` members are invoked 751 times from `tests/Unit`
alone and `CreateContentI18n()` costs 22 containers each time.

```csharp
// tests/Fixtures/Helpers/LocalizerFactory.cs — before
public static class LocalizerFactory
{
    public static T CreateMessage<T>(string culture = "en")
        where T : class
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddScoped<T>();

        ServiceProvider sp = services.BuildServiceProvider();
        using var scope = new CultureScope(culture);
        return sp.GetRequiredService<T>();
    }
}
```

```csharp
// after
public static class LocalizerFactory
{
    private static readonly ConcurrentDictionary<Type, object> Cache = new();

    private static readonly IServiceProvider Provider = new ServiceCollection()
        .AddLogging()
        .AddLocalization()
        .BuildServiceProvider();

    /// <summary>
    /// Returns the shared <typeparamref name="T" /> message instance, backed by the
    /// real embedded .resx resources. One instance per type is built for the
    /// process; these message classes are stateless wrappers over
    /// <see cref="IStringLocalizer{T}" />, which is thread-safe.
    /// </summary>
    /// <typeparam name="T">
    /// The message class to resolve, for example <c>ValidationErrorMessage</c>.
    /// </typeparam>
    /// <remarks>
    /// The returned instance resolves each string at access time against
    /// <see cref="CultureInfo.CurrentUICulture" />. There is no culture parameter,
    /// because scoping the construction of a localizer does not scope its lookups.
    /// To assert a specific translation, wrap the assertion — not this call — in a
    /// <see cref="CultureScope" />:
    /// <code>
    /// using var _ = new CultureScope("fr");
    /// errors.EmailRequired().Message.Should().Be("L'adresse e-mail est requise.");
    /// </code>
    /// </remarks>
    public static T CreateMessage<T>()
        where T : class =>
        (T)Cache.GetOrAdd(typeof(T), static type => ActivatorUtilities.CreateInstance(Provider, type));
}
```

```csharp
// tests/Fixtures/Helpers/TestLocalizer.cs — after
public static class TestLocalizer
{
    private static readonly IStringLocalizerFactory Factory = new ResourceManagerStringLocalizerFactory(
        new OptionsWrapper<LocalizationOptions>(new LocalizationOptions()),
        NullLoggerFactory.Instance
    );

    /// <summary>
    /// Returns a real <see cref="IStringLocalizer{T}" /> over the embedded .resx
    /// resources in the assembly containing <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">
    /// The message class whose assembly carries the matching .resx resource.
    /// </typeparam>
    /// <remarks>
    /// Strings resolve at access time against the ambient UI culture. Pin the
    /// culture around the assertion with a <see cref="CultureScope" />, not around
    /// this call.
    /// </remarks>
    public static IStringLocalizer<T> For<T>()
        where T : class => (IStringLocalizer<T>)Factory.Create(typeof(T));
}
```

Then fix the one call site that tried to use the parameter. Its doc comment is the
most dangerous part of the defect, because it tells the next reader the problem is
already solved:

```csharp
// tests/Fixtures/Helpers/TestErrorsFactory.cs:257-263 — before
/// <summary>
/// Creates a real <see cref="StreamingLinkErrors"/> instance backed by the English catalog.
/// </summary>
public static StreamingLinkErrors CreateStreamingLinkErrors()
{
    return new StreamingLinkErrors(LocalizerFactory.CreateMessage<StreamingLinkErrorMessage>("en"));
}
```

```csharp
// after
/// <summary>
/// Creates a real <see cref="StreamingLinkErrors" /> instance over the embedded
/// resources. The catalogue used is whichever the ambient UI culture selects at
/// the moment a message is read; a test that needs a specific one wraps its
/// assertion in a <see cref="CultureScope" />.
/// </summary>
public static StreamingLinkErrors CreateStreamingLinkErrors()
{
    return new StreamingLinkErrors(LocalizerFactory.CreateMessage<StreamingLinkErrorMessage>());
}
```

*If done wrong:* keeping the `culture` parameter with a default and ignoring it
leaves the same lie in the signature. Remove the parameter so every call site that
passed one becomes a compile error and gets looked at.

### 4. Keep and strengthen the end-to-end localization tests

**Files:** `tests/Integration/Modules/Identity/Application/Auth/UseCases/Public/Commands/Login/V1/PublicLoginEndpointV1Tests.cs`
and the other integration tests that send an `Accept-Language` header.

These are the only tests in the suite that prove the request-localization pipeline
works, because they cross a real HTTP boundary and read the rendered body. The
`en`/`fr` pair at `PublicLoginEndpointV1Tests.cs:259-315` is the model:

```csharp
// tests/Integration/.../Login/V1/PublicLoginEndpointV1Tests.cs:292-315
[Fact]
public async Task Login_WithNonExistentCredentials_InFrench_ReturnsLocalizedFriendlyDetail()
{
    Client.ClearAuthentication();
    const string email = "fantome-fr@nowhere.com";
    var request = new PublicLoginRequestBuilder()
        .WithCredentials(email)
        .WithPassword(TestAuth.ValidPassword)
        .Build();

    var httpRequest = new HttpRequestMessage(HttpMethod.Post, Routes.Public.Auth.Login())
    {
        Content = JsonContent.Create(request),
    };
    httpRequest.Headers.Add("Accept-Language", "fr");

    var response = await Client.SendAsync(httpRequest);

    await response.ShouldBeProblem(HttpStatusCode.NotFound);

    ProblemDetails problem = await response.ReadAsAsync<ProblemDetails>();
    problem.Detail.Should().Contain("Impossible de trouver");
    problem.Detail.Should().Contain("compte utilisateur");
    problem.Detail.Should().NotContain(email);
}
```

The expected fragments are literals, the culture arrives as a request header rather
than as ambient thread state, and an emptied `fr` catalogue fails it. Keep the test —
it is the only proof the pipeline works — and extend it.

Three extensions.

**First, convert the pair to the typed error assertion** once
[04-error-assertion-discipline.md](04-error-assertion-discipline.md) has landed, so it
pins the status, the `Title` and the exact localized `Detail` in both languages instead
of two `Contain` fragments:

```csharp
await response.ShouldBeProblem<NotFoundException>(
    HttpStatusCode.NotFound,
    Localized<SharedExceptionMessage>(m => m.EntityNotFound("User"), LocalizedMessage.EnglishCulture)
);
```

**Second, add a header-absent case, which nothing in the suite covers.** The default
request culture is `fr`:

```csharp
// src/Shared/Shared/Application/Extensions/LocalizationExtension.cs:17-22
private static readonly string[] SupportedCultures = ["fr", "en"];

private const string DefaultCulture = "fr";
```

`AcceptLanguageHeaderRequestCultureProvider` is the only registered provider
(`LocalizationExtension.cs:41`), so a request with no `Accept-Language` header is
answered in French. All thirteen error assertions that expect English prose are in files
that set the header first, and the one French assertion sets it too — the audit found no
test anywhere that sends no header and asserts the resulting language. Nothing has ever
proved the default. A request for a missing article
with no header returns `Impossible de trouver l'article demandé.`; the same request with
`Accept-Language: en` returns `Could not find the requested article.` Both belong in the
theory below.

```csharp
[Theory]
[InlineData(null, "Impossible de trouver")]
[InlineData("fr", "Impossible de trouver")]
[InlineData("en", "Could not find")]
public async Task ArticleNotFound_ReturnsDetailInTheCultureTheRequestSelects(
    string? acceptLanguage,
    string expectedFragment
)
```

The `null` row is the one that matters: it is the only assertion in the suite that would
fail if `SetDefaultCulture` were changed or the provider list were reordered.

**Third, add one equivalent `en`/`fr` pair per *module* boundary** — one for Content, one
for Mailer — so that a module whose satellite assembly fails to build is caught. Three
pairs is enough; this layer proves the pipeline, and change 1 proves the catalogue.

*If done wrong:* asserting a whole sentence rather than a distinctive fragment makes
these tests fail on copy edits. Assert the fragment that identifies the language.

## Expected fallout

**Change 1 is expected to fail on its first run, and that is the deliverable.** It
is the first test in the project's history that can detect a missing or stale
translation across 33 catalogues. Triage what it reports into three buckets:

- A neutral key with no `fr` entry. That is a missing translation and needs a
  translator, not a test change. File it; if the backlog is long, `[Trait]` the
  offending catalogue and track it, but do not delete the assertion.
- A stale `fr` key with no neutral counterpart. Delete the stale entry.
- A placeholder mismatch. That is a latent `FormatException` in production and
  should be fixed immediately.

**The unit test count drops by roughly 200.** 208 executions are deleted and about
70 are added (33 catalogues × 2 theories, plus the discovery fact). The count going
down while the protection goes up is the expected shape of this spec.

**Change 3 will not turn anything red, and that is worth stating.** No test can
currently observe the `culture` parameter's effect, because it has none. The only
compile errors will be at call sites passing an argument;
`grep -rn "CreateMessage<.*>(\"" tests/` finds them, and `TestErrorsFactory.cs:262`
is the only one today.

**Change 3 will make the unit suite measurably faster.** One execution of every
`TestErrorsFactory` call site currently costs 6,209 `ServiceProvider`
constructions, and 160 of the call sites are field initialisers that xUnit re-runs
per `[Fact]`. After the change it costs one.

## Testing

```bash
dotnet test tests/Unit
dotnet test tests/Integration
```

The new theory must be green before the spec is closed. If translations are
genuinely missing and cannot be supplied in the same change, that is a red suite and
a decision for the team — record it in this spec's implementation notes rather than
weakening the assertion.

Prove the new theory can fail before trusting it:

```bash
# temporarily blank one French value, then run only the new tests
dotnet test tests/Unit --filter "FullyQualifiedName~ResourceCompletenessTests"
```

Blanking one `<value>` in any `*.fr.resx` must produce a failure naming that
catalogue and key. Deleting the whole `*.fr.resx` must fail
`EveryNeutralKey_ShouldBePresentAndPopulatedInEveryCulture` on the key-set
comparison, not silently pass through a parent-culture fallback. Run both mutations;
the second is the one that proves `tryParents: false` is doing its job.

Grep-provable invariants after this spec:

```bash
grep -rn "ShouldBeLocalizedForCulture" tests/          # → nothing
grep -rn "CreateMessage<.*>(\"" tests/                 # → nothing
grep -rn "TestLocalizer.For<.*>(\"" tests/             # → nothing
grep -rn "Thread.CurrentThread.CurrentUICulture" tests/ # → nothing (with spec 02)
```

## Risks

**The theory could be too strict for `fr`, and the allowlist could become a
dumping ground.** `FrenchCatalogue_ShouldNotRepeatTheNeutralEnglishString` fails for
any key whose French value happens to equal the English. Some of those are genuine —
a product name, a bare `{0}`. `IdenticalByDesign` exists for them and starts empty
deliberately. Require a one-line justification comment per entry in review; an
allowlist that grows without justification restores the original problem in a new
shape.

**Deleting 104 test methods across 104 files is a wide diff and a review burden.**
Do it as its own commit, separate from change 1, so a reviewer can confirm by
inspection that only the culture theory was removed from each file. Compare
`dotnet test --list-tests` output before and after and check that the delta is
exactly the 208 executions.

**Resource discovery by manifest inspection depends on build output.** If a
`.resx` is ever marked with a build action other than `EmbeddedResource`, or a
satellite assembly is not copied to the test output directory, the theory sees
fewer catalogues than exist.
`Catalogues_ShouldDiscoverEveryShippedResourceFamily` is the guard, and its
hard-coded 33 must be updated deliberately when a catalogue is added — that
friction is the point.

**Caching one message instance per process is a behaviour change in the fixtures.**
It is safe because these classes hold only an `IStringLocalizer<T>` and
`ResourceManagerStringLocalizer` is thread-safe. If a future message class acquires
mutable state, the cache becomes a cross-test leak. Note the constraint in the
`LocalizerFactory` doc comment, which the version above does.

**Three end-to-end pairs is a deliberate floor, not coverage.** They prove the
pipeline resolves `Accept-Language`, nothing more. Resisting the urge to add one per
endpoint is part of this spec: that path leads back to 104 files.

## Checklist

- [ ] 1 — `ResourceCompletenessTests` discovers all 33 catalogues by manifest
      inspection, asserts key-set equality against `en` and `fr` with
      `tryParents: false`, asserts non-empty values, asserts placeholder parity, and
      asserts French does not repeat the neutral English string
- [ ] 2 — all 104 `*_ShouldBeLocalizedForCulture` theories deleted, the surrounding
      files otherwise intact
- [ ] 3 — `LocalizerFactory.CreateMessage<T>` and `TestLocalizer.For<T>` take no
      `culture` parameter and resolve from a single cached container;
      `TestErrorsFactory.CreateStreamingLinkErrors` no longer claims a catalogue it
      cannot pin
- [ ] 4 — the `PublicLoginEndpointV1Tests` `en`/`fr` pair is intact, and one
      equivalent pair exists for Content and for Mailer
