# Medium-High — The localizer helpers scope culture around construction, and leak a container per call

Both test localizer helpers accept a `culture` parameter, wrap a `CultureScope` around the
construction of a localizer, and return. `ResourceManagerStringLocalizer` resolves strings
in its indexer, not in its constructor, so by the time any string is read the scope has
been disposed and the culture restored. The parameter has never had an effect. The same
two methods also build and abandon a full `ServiceProvider` on every call, and they are
called several thousand times per run.

## The problem

### Defect 1 — the culture scope closes before the first string is read

```csharp
// tests/Fixtures/Helpers/LocalizerFactory.cs:25-36
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
```

```csharp
// tests/Fixtures/Helpers/TestLocalizer.cs:26-33
public static IStringLocalizer<T> For<T>(string culture = "en")
    where T : class
{
    var options = new OptionsWrapper<LocalizationOptions>(new LocalizationOptions());
    var factory = new ResourceManagerStringLocalizerFactory(options, NullLoggerFactory.Instance);
    using var scope = new CultureScope(culture);
    return (IStringLocalizer<T>)factory.Create(typeof(T));
}
```

`CultureScope` is a correct, well-written helper — it sets the culture and restores the
previous value on dispose:

```csharp
// tests/Fixtures/Helpers/CultureScope.cs:19-28
public CultureScope(string cultureName)
{
    _previous = CultureInfo.CurrentUICulture;
    CultureInfo.CurrentUICulture = new CultureInfo(cultureName);
}

public void Dispose()
{
    CultureInfo.CurrentUICulture = _previous;
}
```

The `using` declaration disposes at the end of the enclosing method. Both helpers *return*
at the end of that method, so the scope closes as control leaves. What the scope covered
was `GetRequiredService<T>()` and `factory.Create(typeof(T))` — object construction.

`ResourceManagerStringLocalizer` does not capture a culture at construction. It reads
`CultureInfo.CurrentUICulture` inside its indexer, on every lookup. The lookup happens
later, in the test body, after the scope has restored the previous culture. The `culture`
argument therefore changes nothing that any caller can observe.

### The one call site that tried to use it

Of the 29 `LocalizerFactory.CreateMessage` calls inside `TestErrorsFactory`, exactly one
passes an argument:

```csharp
// tests/Fixtures/Helpers/TestErrorsFactory.cs:257-263
/// <summary>
/// Creates a real <see cref="StreamingLinkErrors"/> instance backed by the English catalog.
/// </summary>
public static StreamingLinkErrors CreateStreamingLinkErrors()
{
    return new StreamingLinkErrors(LocalizerFactory.CreateMessage<StreamingLinkErrorMessage>("en"));
}
```

Someone needed a determinism guarantee, reached for the parameter the helper advertises,
and wrote a doc comment asserting the result. The instance is not backed by the English
catalog. It is backed by whatever `CultureInfo.CurrentUICulture` happens to be on the
thread at the moment a message is read — which, under xUnit's default parallelism, is
whatever some other test last set it to. The doc comment is the most dangerous part of
the defect: it tells the next reader the problem is already solved.

### Defect 2 — one `ServiceProvider` per message, never disposed

`CreateMessage<T>` builds a container with `AddLogging()` and `AddLocalization()` on every
call. Neither `sp` nor anything it holds is disposed. `AddLogging` registers an
`ILoggerFactory`; `AddLocalization` registers a `ResourceManagerStringLocalizerFactory`
with its own `ConcurrentDictionary` resource cache and, transitively, a `ResourceManager`
per resource type.

`TestErrorsFactory` fans that out:

```csharp
// tests/Fixtures/Helpers/TestErrorsFactory.cs:229-255
public static ContentI18n CreateContentI18n()
{
    return new ContentI18n(
        CreateArticleErrors(),
        CreateVideoErrors(),
        // ... 20 more
        CreateStreamingLinkErrors()
    );
}
```

22 `Create*Errors()` calls, each of which calls `CreateMessage` at least once. One
`CreateContentI18n()` costs 22 containers.

| Entry point | Containers per call | Call sites |
| --- | --- | --- |
| `CreateContentI18n()` | 22 | 231 in `tests/Unit` |
| `CreateIdentityI18n()` | 5 | 89 in `tests/Unit` |
| `CreateCoreI18n()` | 2 | 42 in `tests/Unit` |
| `CreateUserErrors()` | 4 | 45 in `tests/Unit` |
| all `TestErrorsFactory` members | — | 751 in `tests/Unit`, 14 in `tests/Integration`, 22 inside `tests/Fixtures` |

Summed transitively, one execution of every call site is **6,209 `ServiceProvider`
constructions**. That is the floor, not the figure. 160 of the unit call sites are field
initialisers of the form `private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();`,
and xUnit constructs a new test class instance for every `[Fact]` in it, so those 160 run
once per test method rather than once per class.

## Why it matters

**Nothing in the suite can pin a culture through these helpers.** The declared mechanism
for asserting a French message is `LocalizerFactory.CreateMessage<T>("fr")`, and it does
not work. This compounds the localization finding in
[unit/01](../unit/01-assertions-that-cannot-fail.md): 104 files compare a localized string
against the same localizer the code under test uses, and the one helper that looks like it
could have broken that tie is inert.

**The culture that is actually in effect is whatever another test set.** `CultureScope`
mutates `CultureInfo.CurrentUICulture`, which in .NET flows to new threads via
`CultureInfo.DefaultThreadCurrentUICulture` semantics and is otherwise per-thread state
shared with every other test on the same thread pool worker. A test that legitimately
sets `fr` for its own assertion changes what a concurrently running message lookup
resolves. That is the leakage documented in
[unit/03](../unit/03-culture-and-environment-leakage.md), and these helpers are one of its
sources.

**The allocation cost is paid on every test, and it is not small.** Each abandoned
container holds a logger factory, a localizer factory, and a resource cache, all reachable
from the provider's disposables list until the provider itself is collected. Nothing
disposes them, so they survive to gen 2. On a suite already running against a CI session
timeout ([integration/06](../integration/06-parallelism-and-runtime.md)), several thousand
redundant container builds per run is measurable time spent producing an object that is
byte-for-byte identical every call.

## The fix

Cache one container for the process, resolve messages from it, and remove the parameter
that never worked.

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

// after
public static class LocalizerFactory
{
    private static readonly ConcurrentDictionary<Type, object> Cache = new();

    private static readonly IServiceProvider Provider = new ServiceCollection()
        .AddLogging()
        .AddLocalization()
        .BuildServiceProvider();

    /// <summary>
    /// Returns the shared <typeparamref name="T"/> message instance, backed by the real
    /// embedded .resx resources.
    /// </summary>
    /// <remarks>
    /// The instance resolves each string at access time against
    /// <see cref="CultureInfo.CurrentUICulture"/>. To assert a specific translation, wrap
    /// the assertion — not this call — in a <see cref="CultureScope"/>:
    /// <code>
    /// using var _ = new CultureScope("fr");
    /// errors.EmailRequired().Message.Should().Be("L'adresse e-mail est requise.");
    /// </code>
    /// </remarks>
    public static T CreateMessage<T>()
        where T : class =>
        (T)Cache.GetOrAdd(
            typeof(T),
            static type => ActivatorUtilities.CreateInstance(Provider, type)
        );
}
```

Three properties follow. The container is built once. `T` is constructed once per type and
shared, which is safe because these message classes are stateless wrappers over
`IStringLocalizer<T>` and `ResourceManagerStringLocalizer` is thread-safe. And the doc
comment now tells the truth about where the culture comes from, which is the part that
prevents the next `CreateStreamingLinkErrors` from being written.

`TestLocalizer.For<T>` takes the same treatment — drop the parameter, cache the factory:

```csharp
// tests/Fixtures/Helpers/TestLocalizer.cs — after
public static class TestLocalizer
{
    private static readonly IStringLocalizerFactory Factory = new ResourceManagerStringLocalizerFactory(
        new OptionsWrapper<LocalizationOptions>(new LocalizationOptions()),
        NullLoggerFactory.Instance
    );

    /// <summary>
    /// Returns a real <see cref="IStringLocalizer{T}"/> over the embedded .resx resources
    /// in the assembly containing <typeparamref name="T"/>. Strings resolve at access
    /// time against the ambient UI culture; pin it around the assertion with a
    /// <see cref="CultureScope"/>.
    /// </summary>
    public static IStringLocalizer<T> For<T>()
        where T : class => (IStringLocalizer<T>)Factory.Create(typeof(T));
}
```

Then fix the one call site that tried to pin a culture, and let its test do the pinning:

```csharp
// tests/Fixtures/Helpers/TestErrorsFactory.cs — after
/// <summary>
/// Creates a real <see cref="StreamingLinkErrors"/> instance over the embedded resources.
/// </summary>
public static StreamingLinkErrors CreateStreamingLinkErrors()
{
    return new StreamingLinkErrors(LocalizerFactory.CreateMessage<StreamingLinkErrorMessage>());
}
```

```csharp
// in the test that needs a specific catalog
[Theory]
[InlineData("en", "Streaming link already exists for this platform.")]
[InlineData("fr", "Un lien de streaming existe déjà pour cette plateforme.")]
public void DuplicatePlatform_ShouldReturnMessageInRequestCulture(string culture, string expected)
{
    StreamingLinkErrors errors = TestErrorsFactory.CreateStreamingLinkErrors();

    using var _ = new CultureScope(culture);

    errors.DuplicatePlatform().Message.Should().Be(expected);
}
```

The scope now wraps the *access*, which is the only place the culture is read.

## The principle

**A helper that takes a parameter must use it, and a scope must cover the operation it is
scoping.** `IStringLocalizer` is a late-binding abstraction by design: it resolves at read
time so that a request culture set by middleware applies to strings produced anywhere
downstream. A test helper that scopes construction is scoping the wrong half of that
contract, and the mistake is invisible because the return value looks correct in every
respect except the one being asserted.

The second half is ownership of expensive setup. A container, a resource cache and a
logger factory are process-scoped resources. Building one per call and abandoning it is
not a performance detail — it is a statement that the helper does not know what it owns,
and it is why the same object is rebuilt several thousand times per run.

## Checklist

- [ ] `LocalizerFactory.CreateMessage<T>` has no `culture` parameter and resolves from a
      single cached container
- [ ] `TestLocalizer.For<T>` has no `culture` parameter and reuses one
      `ResourceManagerStringLocalizerFactory`
- [ ] `TestErrorsFactory.CreateStreamingLinkErrors` no longer passes `"en"`, and its doc
      comment no longer claims a catalog it cannot pin
- [ ] Every test that asserts a specific translation wraps the *assertion* in a
      `CultureScope`
- [ ] `grep -rn "CreateMessage<.*>(\"" tests/` returns nothing
