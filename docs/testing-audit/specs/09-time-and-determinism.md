# Spec 09 — Time and determinism

## Goal

`src/` has no clock abstraction: `TimeProvider` and `IClock` appear zero times
across the whole of it, against 60 `DateTime.UtcNow` and 19 `DateTimeOffset.UtcNow`
call sites. Because production code reads the real clock, every test of
time-dependent behaviour has to race it — by sleeping, by fudging a tolerance, or by
betting on the calendar. The suite pays 6.2 seconds of deliberate `Task.Delay` per
run, carries about 30 tolerance-based assertions, orders integration fixtures with
`await Task.Delay(50)`, and holds one assertion that begins failing on 1 January
2030. This spec introduces `TimeProvider` at the points in `src/` that stamp
timestamps, adopts `FakeTimeProvider` in the tests that depend on those points, and
removes every sleep and every calendar bet.

## Scope

In scope:

- `TimeProvider` injected into the `src/` components that read the clock, registered
  as `TimeProvider.System` in `Program.cs`.
- `Microsoft.Extensions.TimeProvider.Testing` added to the two test projects, and
  `FakeTimeProvider` adopted at the ~30 tolerance-based assertion sites.
- The two `Task.Delay(3100)` sleeps in `LoggingDecoratorTests` and the
  `Thread.Sleep(10)` pairs in `AuditableEntityInterceptorTests` and
  `OtpServiceTests`.
- The three `await Task.Delay(50)` ordering sites in `tests/Integration`.
- The 2030 literal in `AdminGetAllSessionsEndpointV1Tests` and the year-rollover
  computations in `AdminUpdateLyricsMetadataValidatorTests`.

Not in this spec:

- `DateTime.UtcNow` inside builders (`VideoBuilder.cs:304` and eleven siblings). The
  builders take an explicit instant as part of this spec's change 4 only where an
  ordering test needs it; the general sweep belongs with spec 08's fixture work.
- The `IDomainEvent.CreatedAt` defect. `src/Shared/Shared/Domain/IDomainEvent.cs:18`
  declares `public DateTime CreatedAt => DateTime.Now;` as a default interface
  implementation, so it returns a *new local-time* value on every access and is not
  a stable timestamp at all. That is a production defect, it is local time rather
  than UTC, and it needs its own ticket alongside spec 13's two. This spec fixes the
  test that works around it, not the property.
- The `Stopwatch` in `LoggingDecorator`. See the note under change 1.
- Making every `src/` clock read go through `TimeProvider`. See "What this spec does
  not convert" below.

## Prerequisites

- **Spec 08 (fixture architecture)** must land first. Change 4 needs
  `ArticleBuilder` to accept a `PublishedAt` override, and adding a builder method
  that tests can actually reach depends on the builders being `public`.
- **Spec 01 (test host fidelity)** for change 5, because the session filter test
  runs against the integration host.

## Decision recorded

The index offers two options and recommends the first. **The recorded decision is to
introduce `TimeProvider` into `src/`.**

The alternative — confining the fix to tests — was rejected because it cannot
succeed. Without a seam, `Handle_WithSlowRequest_ShouldLogPerformanceWarning` has no
way to exceed a three-second threshold except by taking three seconds, and
`CalculateExpirationTime` has no way to produce a known instant. Confining the fix
leaves roughly 30 tolerance-based assertions and 6.2 seconds of sleeps permanently,
and leaves the boundary conditions untestable in the negative direction: today
nothing asserts that a 2.9-second request does *not* log a warning, because
expressing it would add another three seconds to the run.

**This is the one place in the spec set that changes `src/` for testability, and the
ground rules require the justification to be explicit.** Three points make it
defensible:

1. **It adds no dependency.** `TimeProvider` is in the BCL as of .NET 8; the project
   targets .NET 9.
2. **It does not change behaviour.** `TimeProvider.System.GetUtcNow()` returns the
   same value `DateTimeOffset.UtcNow` returns. The production registration is a
   singleton of the system implementation, so the running application reads the same
   clock it reads today.
3. **The codebase already contains the pattern and demonstrates its value.**
   `ContentOrderEntity.MarkPaid` takes `verifiedAt` as a parameter, and its test at
   `tests/Unit/Modules/Content/Domain/Entities/ContentOrderEntityTests.cs:243-261`
   asserts an exact instant, asserts the millisecond-truncation contract explicitly,
   and needs no sleep, no tolerance and no `BeCloseTo`. That test is the target
   shape for every temporal test in the suite, and the reason it can exist is the
   parameter.

Everything else in this spec set is test-only. This one item is not, and it is
called out here so a reviewer can accept or reject it on its own terms rather than
discovering it inside a large diff.

## Changes

### 1. Introduce `TimeProvider` in `src/`

**Files:** `src/Api/Program.cs`, plus the services, handlers, factories and
validators that currently read the clock.

Register the system implementation once, next to the other framework
registrations in `Program.cs`:

```csharp
// src/Api/Program.cs — after builder.Services.AddMemoryCache();
builder.Services.AddSingleton(TimeProvider.System);
```

Then inject it wherever a component reads the clock. `OtpService` is the smallest
complete example:

```csharp
// src/Modules/Identity/Identity/Infrastructure/Services/OtpService.cs:47-50 — before
public DateTime CalculateExpirationTime()
{
    return DateTime.UtcNow.AddMinutes(value: UserConstants.OtpExpirationMinutes);
}
```

```csharp
// after — the primary constructor at OtpService.cs:13 gains the provider
public class OtpService(IPasswordService passwordService, TimeProvider timeProvider) : IOtpService
{
    /// <inheritdoc />
    public DateTime CalculateExpirationTime()
    {
        return timeProvider.GetUtcNow().UtcDateTime.AddMinutes(value: UserConstants.OtpExpirationMinutes);
    }
}
```

The audit interceptor is the highest-value single conversion, because it stamps
every row the application writes:

```csharp
// src/Shared/Shared/Infrastructure/interceptors/AuditableEntityInterceptor.cs:15 — before
public class AuditableEntityInterceptor(ICurrentActor currentActor) : SaveChangesInterceptor
```

```csharp
// after
public class AuditableEntityInterceptor(ICurrentActor currentActor, TimeProvider timeProvider)
    : SaveChangesInterceptor
```

```csharp
// AuditableEntityInterceptor.cs:65-81 — before
foreach (EntityEntry<IEntity> entry in eventDataContext.ChangeTracker.Entries<IEntity>())
{
    if (entry.State == EntityState.Added)
    {
        entry.Entity.CreatedBy = actor;
        entry.Entity.CreatedAt = DateTime.UtcNow;
    }
    // ...
    entry.Entity.UpdatedBy = actor;
    entry.Entity.UpdatedAt = DateTime.UtcNow;
}
```

```csharp
// after — one instant for the whole save, which is also more correct
DateTime now = timeProvider.GetUtcNow().UtcDateTime;

foreach (EntityEntry<IEntity> entry in eventDataContext.ChangeTracker.Entries<IEntity>())
{
    if (entry.State == EntityState.Added)
    {
        entry.Entity.CreatedBy = actor;
        entry.Entity.CreatedAt = now;
    }
    // ...
    entry.Entity.UpdatedBy = actor;
    entry.Entity.UpdatedAt = now;
}
```

Reading the clock once per save rather than twice per entity is a small correctness
improvement in its own right: today two entities saved in the same transaction can
carry different `CreatedAt` values.

`LoggingDecorator` is the case that unlocks the biggest test win:

```csharp
// src/Shared/Shared/Application/Decorators/LoggingDecorator.cs:13-16 — before
public class LoggingDecorator<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> handler,
    ILogger<LoggingDecorator<TRequest, TResponse>> logger
) : IRequestHandler<TRequest, TResponse>
```

```csharp
// after
public class LoggingDecorator<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> handler,
    ILogger<LoggingDecorator<TRequest, TResponse>> logger,
    TimeProvider timeProvider
) : IRequestHandler<TRequest, TResponse>
```

```csharp
// LoggingDecorator.cs:27-33 — before
var stopwatch = Stopwatch.StartNew();

TResponse response = await handler.Handle(request, cancellationToken);

stopwatch.Stop();

LogPerformanceWarning(stopwatch.Elapsed);
```

```csharp
// after
long startTimestamp = timeProvider.GetTimestamp();

TResponse response = await handler.Handle(request, cancellationToken);

LogPerformanceWarning(timeProvider.GetElapsedTime(startTimestamp));
```

`TimeProvider.GetTimestamp` and `GetElapsedTime` are the abstraction's
`Stopwatch` equivalent, and `FakeTimeProvider` advances them with `Advance`. This is
why the decorator converts to `TimeProvider` rather than keeping `Stopwatch` — the
elapsed measurement is the thing the test needs to control.

**Domain entities take the instant as a parameter rather than an injected service.**
`ContentOrderEntity.MarkPaid(paymentId, verifiedAt, ...)` already does this and is
the pattern. Where an entity method currently stamps its own timestamp — such as
`ArticleEntity.Publish()` setting `PublishedAt = DateTimeOffset.UtcNow` at
`src/Modules/Content/Content/Domain/Entities/ArticleEntity.cs:453` — add the instant
as a parameter and let the handler supply `timeProvider.GetUtcNow()`. Do not inject
a service into an aggregate.

**What this spec does not convert.** 24 of the 60 `DateTime.UtcNow` sites are inside
`src/Modules/Content/Content/Domain/Entities/`, and converting all of them means
changing 24 domain method signatures and every one of their callers. Convert only the
ones a test in changes 2 through 5 needs, and record the remainder as follow-up. A
partial seam is still a seam; a 24-signature change landed alongside four other
changes is not reviewable.

*If done wrong:* registering `TimeProvider.System` as anything other than a singleton,
or forgetting the registration entirely, produces a DI resolution failure at the
first request rather than a compile error. Add a registration assertion to the
existing module registration tests.

### 2. Adopt `FakeTimeProvider` in the tests

**Files:** `tests/Unit/_116.Unit.Tests.csproj`,
`tests/Integration/_116.Integration.Tests.csproj`, and the ~30 tolerance-based
assertion sites.

Add the package to both test projects. The 9.x band targets .NET 9; confirm the
current 9.x release before pinning rather than copying this line:

```xml
<PackageReference Include="Microsoft.Extensions.TimeProvider.Testing" Version="9.10.0" />
```

Group it with the existing mocking and data-generation references and do not add an
XML separator comment for it.

The `LoggingDecorator` tests become instantaneous and exact:

```csharp
// tests/Unit/Shared/Application/Decorators/LoggingDecoratorTests.cs:221-233 — before
[Fact]
public async Task Handle_WithSlowRequest_ShouldLogPerformanceWarning()
{
    // Arrange
    Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
    handlerMock
        .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
        .Returns(async () =>
        {
            await Task.Delay(3100); // Delay more than 3 seconds
            return new TestResponse("Success");
        });
    ...
}
```

```csharp
// after
[Theory]
[InlineData(3.1, 1)]
[InlineData(2.9, 0)]
public async Task Handle_ShouldLogPerformanceWarningOnlyAboveTheThreeSecondThreshold(
    double elapsedSeconds,
    int expectedWarnings
)
{
    // Arrange
    FakeTimeProvider time = new();
    Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
    handlerMock
        .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
        .Returns(() =>
        {
            time.Advance(TimeSpan.FromSeconds(elapsedSeconds));
            return Task.FromResult(new TestResponse("Success"));
        });

    LoggingDecorator<TestRequest, TestResponse> decorator = new(
        handlerMock.Object,
        _loggerMock.Object,
        time
    );

    // Act
    await decorator.Handle(new TestRequest("test"));

    // Assert
    _loggerMock.Verify(
        x =>
            x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
        Times.Exactly(expectedWarnings)
    );
}
```

The `2.9` case is new and is the half that makes the threshold a threshold. The
current sleep-based test cannot express it without adding three more seconds to the
run, which is why it does not exist.

The same treatment applies to the ~30 `BeCloseTo` and tolerance sites. Where a test
asserts that a value is near `UtcNow`, supply the instant and assert equality:

```csharp
// tests/Unit/Modules/Identity/Infrastructure/Services/OtpServiceTests.cs:226-235 — before
DateTime expectedExpiration = beforeCall.AddMinutes(UserConstants.OtpExpirationMinutes);
expirationTime.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(2));
```

```csharp
// after
FakeTimeProvider time = new(new DateTimeOffset(2026, 6, 30, 10, 0, 0, TimeSpan.Zero));
OtpService sut = new(_passwordServiceMock.Object, time);

DateTime expirationTime = sut.CalculateExpirationTime();

expirationTime.Should().Be(new DateTime(2026, 6, 30, 10, 0, 0, DateTimeKind.Utc)
    .AddMinutes(UserConstants.OtpExpirationMinutes));
```

`UserConstants.OtpExpirationMinutes` stays aliased rather than inlined — that is
spec 03's rule and it still applies here.

*If done wrong:* a `FakeTimeProvider` constructed per test class rather than per test
carries advancement between `[Fact]` executions on the same instance. xUnit
constructs a new class instance per fact, so a field initialiser is safe; a `static`
field is not.

### 3. Delete the sleeps

**Files:** `tests/Unit/Shared/Application/Decorators/LoggingDecoratorTests.cs`,
`tests/Unit/Shared/Infrastructure/Interceptors/AuditableEntityInterceptorTests.cs`,
`tests/Unit/Modules/Identity/Infrastructure/Services/OtpServiceTests.cs`.

The two `Task.Delay(3100)` calls at `LoggingDecoratorTests.cs:230` and `:263` are
removed by change 2 and cost 6.2 seconds of wall time on every run by every
developer and every CI job. They also leave 100 ms of margin over a 3,000 ms
threshold, which is under 4% headroom on a loaded agent where `Task.Delay`
resolution and thread-pool scheduling both degrade.

`AuditableEntityInterceptorTests` sleeps to force two audit stamps to differ:

```csharp
// tests/Unit/Shared/Infrastructure/Interceptors/AuditableEntityInterceptorTests.cs:104-105 — before
DateTime originalUpdatedAt = entity.UpdatedAt!.Value;
Thread.Sleep(10); // Ensure time difference
```

```csharp
// after
DateTime originalUpdatedAt = entity.UpdatedAt!.Value;
_time.Advance(TimeSpan.FromMinutes(1));
```

`Thread.Sleep` blocks a pool thread while the rest of the suite runs, and 10 ms is
below the resolution the system clock is guaranteed to advance by on all platforms —
so the assertion it enables is not merely slow, it is not guaranteed to hold. The
same edit applies at `:264`, and `await Task.Delay(10)` at `:172` becomes the same
`Advance` call.

`OtpServiceTests.cs:240-247` is a different case and is deleted rather than
converted:

```csharp
// before
[Fact]
public void CalculateExpirationTime_CalledMultipleTimes_ShouldReturnIncreasingTimes()
{
    DateTime time1 = _sut.CalculateExpirationTime();
    Thread.Sleep(10); // Small delay
    DateTime time2 = _sut.CalculateExpirationTime();

    time2.Should().BeOnOrAfter(time1);
}
```

The assertion is `BeOnOrAfter`, which the same value satisfies, so the test passes
whether or not the sleep had any effect. It asserts that the system clock is
monotonic, which is not a property of `OtpService`. Delete it; the converted
`CalculateExpirationTime` test in change 2 covers the method.

*If done wrong:* replacing `Thread.Sleep` with `Task.Delay` keeps the wall time and
adds an `async` signature. The point is to remove the wait, not to move it.

### 4. Seed timestamps in the integration ordering tests

**Files:** `tests/Integration/Modules/Content/Application/Editorial/UseCases/Public/Queries/GetPopularArticles/V1/PublicGetPopularArticlesEndpointV1Tests.cs:88-92`,
`tests/Integration/Modules/Content/Application/Editorial/UseCases/Public/Queries/GetPopularVideos/V1/PublicGetPopularVideosEndpointV1Tests.cs:83`,
`tests/Integration/Shared/Infrastructure/Interceptors/AuditableEntityInterceptorTests.cs:48`,
and `tests/Fixtures/Builders/Entities/Content/ArticleBuilder.cs`.

```csharp
// PublicGetPopularArticlesEndpointV1Tests.cs:88-92 — before
ArticleEntity older = await SeedArticleAsync(categoryId, likes: 2);
await Task.Delay(50);
ArticleEntity newer = await SeedArticleAsync(categoryId, likes: 2);
```

The test asserts `ContainInOrder(newer.Id, older.Id)` on a tie-break by
`PublishedAt` descending, and the ordering it asserts is produced by the sleep rather
than by any seeded value. It fails in both directions: if the tie-break in the query
were reversed the test would notice, but if the two rows land in the same clock tick
the ordering becomes arbitrary and the test flakes. Either way it is asserting a
property of the seeding, not of the query.

`VideoBuilder` already supports the override through
`AsPublishedAt(DateTimeOffset publishedAt)` at `VideoBuilder.cs:197`. `ArticleBuilder`
does not, so add the equivalent:

```csharp
/// <summary>
/// Publishes the article with an explicit publication timestamp, so that a test
/// asserting "latest first" ordering gets its ordering from a seeded value rather
/// than from how fast the seeding ran.
/// </summary>
/// <param name="publishedAt">The publication instant to stamp on the entity.</param>
/// <returns>The same builder, for chaining.</returns>
public ArticleBuilder AsPublishedAt(DateTimeOffset publishedAt)
{
    _targetStatus = EnumContentStatus.Published;
    _publishedAtOverride = publishedAt;
    return this;
}
```

Follow `VideoBuilder.cs:289-297` exactly: apply the override with reflection over
`nameof(ArticleEntity.PublishedAt)` *after* the status transition has run, so the
entity is genuinely published and only the timestamp is backdated. That is the one
legitimate use of reflection under spec 08's rule.

Then thread the parameter through the test's own seeding helper:

```csharp
// after
ArticleEntity older = await SeedArticleAsync(
    categoryId,
    likes: 2,
    publishedAt: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
);
ArticleEntity newer = await SeedArticleAsync(
    categoryId,
    likes: 2,
    publishedAt: new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero)
);
```

`SeedArticleAsync` at `PublicGetPopularArticlesEndpointV1Tests.cs:31-65` calls
`ArticleFactory.CreatePublished(categoryId)`; give it an optional `publishedAt` and
route it to the new builder method when supplied.

*If done wrong:* seeding two timestamps a millisecond apart reproduces the original
problem at a smaller scale. Use months, so the intent is legible and no rounding in
the persistence layer can collapse them.

### 5. Remove the calendar bets

**Files:** `tests/Integration/Modules/Identity/Application/Session/UseCases/Admin/Queries/GetAllSessions/V1/AdminGetAllSessionsEndpointV1Tests.cs:137-157`,
`tests/Unit/Modules/Content/Application/Editorial/UseCases/Admin/Commands/UpdateLyricsMetadata/AdminUpdateLyricsMetadataValidatorTests.cs:96-155`.

The session filter test creates a row at test-run time and filters with a fixed
literal four years in the future:

```csharp
// AdminGetAllSessionsEndpointV1Tests.cs:149-151 — before
var response = await Client.GetAsync(
    $"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&toDate=2030-01-01T00:00:00Z"
);
```

It passes because "now" is before 2030 and starts failing on 1 January 2030, for a
reason unrelated to the `SessionCreatedBeforeSpecification` the test documents itself
as covering. It also has a second defect: **nothing in the arrangement is excluded by
the filter**, so the assertion cannot fail today either. A filter that returns every
row passes.

```csharp
// after — seed one row inside the window and one outside it
var included = new DateTimeOffset(2026, 3, 1, 12, 0, 0, TimeSpan.Zero);
var excluded = new DateTimeOffset(2026, 5, 1, 12, 0, 0, TimeSpan.Zero);
var toDate = included.AddDays(1);

SessionEntity inWindow = await SeedAsync<IdentityDbContext, SessionEntity>(ctx =>
{
    SessionEntity entity = SessionFactory.CreateWithCreatedAt(TestUser.SuperAdminId, included);
    ctx.Sessions.Add(entity);
    return entity;
});

SessionEntity outOfWindow = await SeedAsync<IdentityDbContext, SessionEntity>(ctx =>
{
    SessionEntity entity = SessionFactory.CreateWithCreatedAt(TestUser.SuperAdminId, excluded);
    ctx.Sessions.Add(entity);
    return entity;
});

Client.AuthenticateAsSuperAdmin();

var response = await Client.GetAsync(
    $"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&toDate={toDate:O}"
);

response.StatusCode.Should().Be(HttpStatusCode.OK);

AdminGetAllSessionsResponse body = await response.ReadAsAsync<AdminGetAllSessionsResponse>();
body.Sessions.Items.Should().Contain(s => s.Id == inWindow.Id);
body.Sessions.Items.Should().NotContain(s => s.Id == outOfWindow.Id);
```

The `NotContain` line is the point of the change. Without it the test cannot
distinguish a working `SessionCreatedBeforeSpecification` from a filter that was
silently dropped, which is the same class of defect the query-builder work in spec
05 addresses.

`SessionFactory` has no `CreateWithCreatedAt` today — its members are listed at
`tests/Fixtures/Factories/Identity/SessionFactory.cs:16-200` — and `SessionBuilder`
has no `CreatedAt` setter. Add the builder method first, following the layering rule
from spec 08, and add the factory alias only if three or more tests need it.

The validator tests compute their expected value from the same clock the validator
reads:

```csharp
// AdminUpdateLyricsMetadataValidatorTests.cs:99 — before, asserting invalid
var releaseYear = (short)(DateTimeOffset.UtcNow.Year + 2);

// AdminUpdateLyricsMetadataValidatorTests.cs:141 — before, asserting valid
var releaseYear = (short)(DateTimeOffset.UtcNow.Year + 1);
```

The rule under test is
`src/Modules/Content/Content/Application/Shared/Validators/EditorialValidation.cs:543-548`:

```csharp
return ruleBuilder
    .InclusiveBetween((short)1900, (short)(DateTimeOffset.UtcNow.Year + 1))
    .When(x => GetNullableShortPropertyValue(instance: x, propertyName: "ReleaseYear") is not null);
```

If the test's read and the validator's read fall on opposite sides of midnight on 31
December, the computed year and the validated boundary disagree and the test fails
for one run a year, at the least convenient time. It is also the temporal form of the
tautology rule: the expected value comes from the same source as the actual.

Give `ValidReleaseYear` the clock as a parameter and let the test supply it:

```csharp
// EditorialValidation.cs — after
/// <summary>
/// Constrains a release year to the range 1900 through next year, relative to the
/// supplied clock. The clock is a parameter so that a test can assert both sides of
/// the upper boundary without depending on the calendar date of the run.
/// </summary>
/// <param name="ruleBuilder">The rule builder being extended.</param>
/// <param name="timeProvider">The clock used to compute the upper boundary.</param>
public static IRuleBuilderOptions<T, short?> ValidReleaseYear<T>(
    this IRuleBuilder<T, short?> ruleBuilder,
    TimeProvider timeProvider
)
{
    return ruleBuilder
        .InclusiveBetween((short)1900, (short)(timeProvider.GetUtcNow().Year + 1))
        .When(x => GetNullableShortPropertyValue(instance: x, propertyName: "ReleaseYear") is not null);
}
```

```csharp
// AdminUpdateLyricsMetadataValidatorTests.cs — after
[Theory]
[InlineData((short)1899, false)]
[InlineData((short)1900, true)]
[InlineData((short)2027, true)]
[InlineData((short)2028, false)]
public async Task Validate_ShouldAcceptReleaseYearsFrom1900ThroughNextYear(short releaseYear, bool expected)
{
    FakeTimeProvider time = new(new DateTimeOffset(2026, 6, 30, 0, 0, 0, TimeSpan.Zero));
    AdminUpdateLyricsMetadataValidator validator = new(_i18n, time);

    var command = new AdminUpdateLyricsMetadataCommand(
        Id: Guid.NewGuid(),
        Album: null,
        ReleaseYear: releaseYear,
        Label: null,
        Songwriter: null,
        Producer: null
    );

    ValidationResult result = await validator.ValidateAsync(command);

    result.IsValid.Should().Be(expected);
}
```

Every boundary is now a literal, all four cases are covered in one theory, and the
result does not change on 1 January.

*If done wrong:* pinning `FakeTimeProvider` to a date and leaving the expected years
computed from it reproduces the tautology with extra steps. Both sides must be
literals.

## Expected fallout

**Change 1 touches `src/` and will produce compile errors at every construction site
of every converted type.** That is the intended shape: each error is a place that has
to decide where its clock comes from. Unit tests that `new` a converted service
directly will fail to compile until they supply a provider.

**Registering `TimeProvider` may surface a DI gap in the test host.** The integration
fixture builds the application from `Program.cs`, so the singleton is registered
there. Any test that assembles its own `ServiceCollection` to assert registrations
must add it.

**The unit suite gets roughly 6.2 seconds faster**, and the `LoggingDecorator` tests
stop being the slowest two in the suite. Expect no other timing change; the sleeps in
changes 3 and 4 are milliseconds.

**Change 5 will turn the session filter test red if `SessionCreatedBeforeSpecification`
is not doing its job.** The excluded row is new, and nothing has ever checked that the
filter excludes anything. If it fails, that is a production defect and gets its own
ticket alongside spec 13's.

**Change 5 changes a public extension method signature in `src/`.**
`ValidReleaseYear` gains a `TimeProvider` parameter, so every validator using it must
inject one. There are three callers —
`AdminCreateAlbumValidator.cs:20`, `AdminUpdateAlbumValidator.cs:20` and
`AdminUpdateLyricsMetadataValidator.cs:19` — and each currently takes only
`ContentI18n`. All three gain a second constructor parameter, and their unit tests
gain a `FakeTimeProvider`.

## Testing

```bash
dotnet build
dotnet test tests/Unit
dotnet test tests/Integration
```

Both suites green. Run the integration suite twice back to back: change 4 removes the
sleeps that were masking ordering non-determinism, so a difference between runs is a
real ordering defect rather than a flake.

Measure the improvement, because it is one of this spec's stated goals:

```bash
dotnet test tests/Unit --filter "FullyQualifiedName~LoggingDecoratorTests"
```

That filter should complete in well under a second where it previously took over six.

Grep-provable invariants after this spec:

```bash
# no sleeping in a test body
grep -rn "Task.Delay\|Thread.Sleep" tests/            # → nothing

# no local-time reads in tests
grep -rn "DateTime.Now" tests/                        # → nothing

# no hard-coded future date in an assertion or query string
grep -rn "20[3-9][0-9]-" tests/                       # → nothing

# no expected value computed from the live clock
grep -rn "UtcNow.Year" tests/                         # → nothing

# the production registration exists
grep -n "AddSingleton(TimeProvider.System)" src/Api/Program.cs   # → one hit
```

The new tests that prove the fix, and the mutation each catches:

| New test | Mutation it catches |
| --- | --- |
| `Handle_ShouldLogPerformanceWarningOnlyAboveTheThreeSecondThreshold`, the 2.9 s case | changing the threshold from `> 3` to `>= 0` |
| `Validate_ShouldAcceptReleaseYearsFrom1900ThroughNextYear`, the 2028 case | changing the upper boundary to `Year + 2` |
| `GetAllSessions_FilterByToDate_ReturnsFilteredResults`, the `NotContain` assertion | dropping `SessionCreatedBeforeSpecification` from the query builder |
| `GetPopularArticles_WhenScoresAreEqual_TieBreaksByPublishedAtDescending` | reversing the tie-break to ascending |

Apply the first and the third mutations locally and confirm red. Those two are the
cases where the previous test could not fail at all.

## Risks

**This spec changes `src/`, and every other spec in this set does not.** The
mitigation is the scoping in change 1: convert only what changes 2 through 5 need,
leave the remaining `UtcNow` sites in the domain entities as recorded follow-up, and
land the `src/` change as its own commit so it can be reviewed and reverted
independently of the test work.

**A partial conversion leaves two ways to read the clock in the same codebase.**
That is genuinely worse than one way, and it is temporary by intent. Record the
remaining sites — a list, in this spec's implementation notes, not a vague "the
rest" — and treat any new `DateTime.UtcNow` in a converted file as a review
rejection.

**`FakeTimeProvider` does not advance on its own.** Code that awaits a timer or a
delay under a `FakeTimeProvider` blocks until the test advances it, which turns a
missing `Advance` call into a hung test rather than a failing one. None of the
conversions in this spec await a `TimeProvider`-backed delay, but the next one might;
prefer `Advance` immediately before the assertion, not in a helper.

**Changing `ValidReleaseYear`'s signature ripples into three validators and their
tests.** The album create and update validators are affected even though neither has
a year-boundary test today, so the change lands there as a constructor edit with no
behaviour change. If that ripple grows during implementation, split change 5 into
the session half and the validator half and land them separately.

**`ArticleBuilder.AsPublishedAt` adds a reflection write to the fixtures layer**,
which spec 08 is otherwise reducing. It is the legitimate category — backdating a
timestamp on an entity that has genuinely been published — and it mirrors an existing
`VideoBuilder` method. Apply it after the status transition, use `nameof`, and say
so in the doc comment.

**The `IDomainEvent.CreatedAt` defect stays open after this spec.** `IDomainEventTests`
at `tests/Unit/Shared/Domain/IDomainEventTests.cs:35-49` will still be working around
it with `DateTime.Now` and a 10 ms tolerance. Convert that test to UTC and to an
exact assertion only once the property is fixed; until then, note it and leave it, so
the workaround does not get mistaken for a passing contract.

## Checklist

- [ ] 1 — `TimeProvider.System` registered as a singleton in `Program.cs`; the
      components that changes 2–5 depend on take `TimeProvider` by constructor
      injection; domain methods take the instant as a parameter; the unconverted
      `UtcNow` sites are enumerated in the implementation notes
- [ ] 2 — `Microsoft.Extensions.TimeProvider.Testing` referenced by both test
      projects at a verified 9.x version; the tolerance-based assertions replaced by
      exact ones against a `FakeTimeProvider`
- [ ] 3 — both `Task.Delay(3100)` calls and all three `Thread.Sleep` / `Task.Delay(10)`
      pairs removed; `CalculateExpirationTime_CalledMultipleTimes_ShouldReturnIncreasingTimes`
      deleted
- [ ] 4 — `ArticleBuilder.AsPublishedAt` added; the three `await Task.Delay(50)`
      ordering sites seed explicit timestamps months apart
- [ ] 5 — the 2030 literal replaced by a seeded window that also seeds a row the
      filter must exclude; `ValidReleaseYear` takes a `TimeProvider` and its tests
      assert all four boundary cases against literal years
