# Medium — Wall-clock dependence, no clock seam

`src/` has no clock abstraction. `TimeProvider` and `IClock` appear zero times
across the whole of `src/`, against 60 direct `DateTime.UtcNow` call sites and 19
`DateTimeOffset.UtcNow` call sites. Because production code reads the real clock,
every test of time-dependent behaviour has to race it — by sleeping, by fudging
tolerances, or by asserting against a date that will eventually arrive. The suite
contains 372 `DateTime`/`DateTimeOffset` `.UtcNow`/`.Now` reads, at least 6.2
seconds of deliberate `Task.Delay`, two `Thread.Sleep` calls whose comment is
"Ensure time difference", and one assertion that begins failing on 1 January 2030.

## The problem

### There is no seam

```
TimeProvider / IClock references in src/ : 0
DateTime.UtcNow call sites in src/       : 60
DateTimeOffset.UtcNow call sites in src/ : 19
```

A handler, entity or validator that needs the current time calls `UtcNow` inline.
There is no parameter a test can supply, so there is no way to place the system at a
chosen instant. Everything below follows from that.

### Six seconds of sleeping to cross a threshold

`LoggingDecorator` emits a performance warning when a request exceeds three
seconds, measured with a `Stopwatch`:

```csharp
// src/Shared/Shared/Application/Decorators/LoggingDecorator.cs:27,53-58
var stopwatch = Stopwatch.StartNew();
...
if (elapsed.TotalSeconds > 3)
{
    logger.LogWarning(
        "[PERFORMANCE] Request {Request} took {ElapsedSeconds:N2} seconds.",
        ...
        elapsed.TotalSeconds
    );
}
```

The only way the tests can reach that branch is to actually take longer than three
seconds, twice:

```csharp
// tests/Unit/Shared/Application/Decorators/LoggingDecoratorTests.cs:221-233
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

The identical delay appears again at line 263 in
`Handle_WithSlowRequest_ShouldLogElapsedSeconds`. Together they cost 6.2 seconds of
wall time and leave 100 ms of margin over a 3,000 ms threshold — under 4% headroom
on a loaded CI agent where `Task.Delay` resolution and thread-pool scheduling both
degrade.

### Local time, and a hand-rolled fudge factor

```csharp
// tests/Unit/Shared/Domain/IDomainEventTests.cs:35-49
[Fact]
public void CreatedAt_ShouldBeCurrentDateTime()
{
    // Arrange
    DateTime before = DateTime.Now;

    // Act
    IDomainEvent domainEvent = new TestDomainEvent();
    DateTime createdAt = domainEvent.CreatedAt; // Cache the value since property returns DateTime.Now on each access

    // Assert
    DateTime after = DateTime.Now;
    createdAt.Should().BeOnOrAfter(before);
    createdAt.Should().BeOnOrBefore(after.AddMilliseconds(10)); // Add tolerance for timing precision
}
```

Two problems compound. `DateTime.Now` is local time, so the test's behaviour depends
on the agent's time zone and on whether a DST transition falls inside the window.
And `AddMilliseconds(10)` is a magic tolerance chosen to make the assertion pass,
not derived from any contract — which means the test also passes if `CreatedAt`
becomes wrong by up to 10 ms.

The comment is the more useful finding: `CreatedAt` returns `DateTime.Now` on each
access, so it is not a stable timestamp at all. That is a defect in `IDomainEvent`
that the test documents and then works around.

### Sleeping to force a timestamp difference

```csharp
// tests/Unit/Shared/Infrastructure/Interceptors/AuditableEntityInterceptorTests.cs:105,264
Thread.Sleep(10); // Ensure time difference
```

Both sites exist because the interceptor stamps `UpdatedAt` from the real clock and
the test needs two stamps to differ. `Thread.Sleep` blocks a pool thread while the
rest of the suite runs, and 10 ms is below the resolution the system clock is
guaranteed to advance by on all platforms.

### Racing the validator's own clock read

```csharp
// tests/Unit/.../UpdateLyricsMetadata/AdminUpdateLyricsMetadataValidatorTests.cs:99
var releaseYear = (short)(DateTimeOffset.UtcNow.Year + 2);
```

```csharp
// tests/Unit/.../UpdateLyricsMetadata/AdminUpdateLyricsMetadataValidatorTests.cs:141
var releaseYear = (short)(DateTimeOffset.UtcNow.Year + 1);
```

The first asserts the value is invalid, the second that it is valid. Both compute
their expected value from the same clock the validator reads. If the test's read and
the validator's read fall on opposite sides of midnight on 31 December, the computed
year and the validated boundary disagree and the test fails for one run a year, at
the least convenient time.

### Integration tests racing the clock to build an ordering

```csharp
// tests/Integration/.../GetPopularArticles/V1/PublicGetPopularArticlesEndpointV1Tests.cs:88-92
ArticleEntity older = await SeedArticleAsync(categoryId, likes: 2);
await Task.Delay(50);
ArticleEntity newer = await SeedArticleAsync(categoryId, likes: 2);
```

The test asserts `ContainInOrder(newer.Id, older.Id)` on a tie-break by
`PublishedAt` descending. The ordering it asserts is produced by a 50 ms sleep
rather than by seeded values. The videos sibling has the same construction at
`tests/Integration/.../GetPopularVideos/V1/PublicGetPopularVideosEndpointV1Tests.cs:83`.

### A dated time bomb

```csharp
// tests/Integration/.../GetAllSessions/V1/AdminGetAllSessionsEndpointV1Tests.cs:137-157
[Fact]
public async Task GetAllSessions_FilterByToDate_ReturnsFilteredResults()
{
    SessionEntity session = await SeedAsync<IdentityDbContext, SessionEntity>(ctx =>
    {
        SessionEntity entity = SessionFactory.Create(TestUser.SuperAdminId);
        ctx.Sessions.Add(entity);
        return entity;
    });

    Client.AuthenticateAsSuperAdmin();

    var response = await Client.GetAsync(
        $"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&toDate=2030-01-01T00:00:00Z"
    );

    response.StatusCode.Should().Be(HttpStatusCode.OK);

    AdminGetAllSessionsResponse body = await response.ReadAsAsync<AdminGetAllSessionsResponse>();
    body.Sessions.Items.Should().Contain(s => s.Id == session.Id);
}
```

The session is created at test-run time and the filter upper bound is a fixed
literal. The test passes because "now" is before 2030 and starts failing on
1 January 2030, for a reason unrelated to the `SessionCreatedBeforeSpecification` it
documents itself as covering.

### What is already right

Three findings run the other way and constrain the fix.

- **24 `BeCloseTo` usages.** Where the suite compares against a wall-clock value it
  generally does so with an explicit tolerance rather than exact equality.
- **No exact-equality assertion against `UtcNow` exists anywhere in `tests/`.** A
  grep for `Should().Be(DateTime...` / `Should().Be(DateTimeOffset...` against a
  live clock read returns nothing.
- **`ContentOrderEntityTests` is the model for deterministic time.** It supplies the
  instant, asserts the exact expected instant, and asserts the truncation contract
  explicitly rather than tolerating it:

```csharp
// tests/Unit/Modules/Content/Domain/Entities/ContentOrderEntityTests.cs:243-261
DateTimeOffset verifiedAt = new DateTimeOffset(2026, 6, 30, 10, 15, 42, 123, TimeSpan.Zero).AddTicks(4567);
DateTimeOffset expectedPaidAt = new(2026, 6, 30, 10, 15, 42, 123, TimeSpan.Zero);

order.MarkPaid(
    paymentId: Guid.NewGuid(),
    verifiedAt: verifiedAt,
    promotionDurationsByLevelId: new Dictionary<Guid, int> { [promotionLevelId] = durationDays },
    errors: _errors
);

OrderPaidEvent paidEvent = order.DomainEvents.OfType<OrderPaidEvent>().Should().ContainSingle().Which;
paidEvent.PaidAt.Should().Be(expectedPaidAt);
(paidEvent.PaidAt.Ticks % TimeSpan.TicksPerMillisecond).Should().Be(0);
effect.PromotionUntil.Should().Be(expectedPaidAt.AddDays(durationDays));
```

`MarkPaid` takes `verifiedAt` as a parameter. That single design decision is why its
test needs no sleep, no tolerance and no `BeCloseTo`, and it is the pattern the
other 60 `UtcNow` call sites should follow.

## Why it matters

Wall-clock dependence produces failures that carry no information. When
`Handle_WithSlowRequest_ShouldLogPerformanceWarning` fails on a busy agent, nothing
about `LoggingDecorator` has changed; the agent was slow, or fast, in the wrong
direction. The first such failure gets investigated; the third gets a re-run; after
that the suite's red is discounted, and a real regression arriving in that window is
discounted with it.

The 6.2 seconds is a smaller but real cost, paid on every run by every developer and
every CI job, to test a comparison against a constant.

The year-boundary and 2030 cases are the most dangerous, because they fail *later*.
A test that has passed for four years and fails on 1 January is indistinguishable
from a regression, and will be triaged as one. The team then spends time proving
that nothing changed.

The `Task.Delay(50)` ordering tests fail in the other direction: they are green even
when broken. If the tie-break ordering in the query is reversed, the 50 ms gap makes
the two rows distinguishable but the test only asserts the order it expects; if the
seeding is fast enough that both rows land in the same clock tick, the ordering
becomes arbitrary and the test flakes. Either way it is asserting a property of the
seeding, not of the query.

## The fix

### Introduce `TimeProvider` in `src/`

`TimeProvider` is in the BCL as of .NET 8 and the project targets .NET 9, so this
adds no dependency. Inject it wherever a component currently reads the clock:

```csharp
// Before
public class SomeHandler(ISomeRepository repository, IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(SomeCommand command, CancellationToken ct)
    {
        entity.Expire(DateTimeOffset.UtcNow);
        ...
    }
}
```

```csharp
// After
public class SomeHandler(ISomeRepository repository, IUnitOfWork unitOfWork, TimeProvider timeProvider)
{
    public async Task<Result> Handle(SomeCommand command, CancellationToken ct)
    {
        entity.Expire(timeProvider.GetUtcNow());
        ...
    }
}
```

Register the system implementation once in `Program.cs`:

```csharp
builder.Services.AddSingleton(TimeProvider.System);
```

Domain entities take the instant as a parameter rather than an injected service, as
`ContentOrderEntity.MarkPaid` already does.

### Use `FakeTimeProvider` in tests

`Microsoft.Extensions.TimeProvider.Testing` supplies `FakeTimeProvider`, which
advances only when the test advances it. The `LoggingDecorator` tests become
instantaneous and exact:

```csharp
// Before — tests/Unit/Shared/Application/Decorators/LoggingDecoratorTests.cs:221-233
handlerMock
    .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
    .Returns(async () =>
    {
        await Task.Delay(3100); // Delay more than 3 seconds
        return new TestResponse("Success");
    });
```

```csharp
// After
var time = new FakeTimeProvider();
LoggingDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, loggerMock.Object, time);

handlerMock
    .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
    .Returns(() =>
    {
        time.Advance(TimeSpan.FromSeconds(3.1));
        return Task.FromResult(new TestResponse("Success"));
    });

await decorator.Handle(new TestRequest("test"));

loggerMock.Verify(/* [PERFORMANCE] warning */, Times.Once);
```

That also makes the boundary testable in both directions: advancing 2.9 seconds must
*not* log, which the current sleep-based test cannot express without adding another
three seconds to the run.

The interceptor's `Thread.Sleep(10); // Ensure time difference` becomes
`time.Advance(TimeSpan.FromMinutes(1))`, which is both instant and unambiguous.

### Fix the year-boundary validator tests

```csharp
// Before — AdminUpdateLyricsMetadataValidatorTests.cs:99
var releaseYear = (short)(DateTimeOffset.UtcNow.Year + 2);
```

```csharp
// After — the validator is told what "now" is, so the boundary is a literal
var time = new FakeTimeProvider(new DateTimeOffset(2026, 6, 30, 0, 0, 0, TimeSpan.Zero));
var validator = new AdminUpdateLyricsMetadataValidator(i18n, time);

const short releaseYear = 2028; // maximum is current year + 1

...
```

### Seed timestamps in the integration ordering tests

```csharp
// Before — PublicGetPopularArticlesEndpointV1Tests.cs:88-92
ArticleEntity older = await SeedArticleAsync(categoryId, likes: 2);
await Task.Delay(50);
ArticleEntity newer = await SeedArticleAsync(categoryId, likes: 2);
```

```csharp
// After
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

`ArticleBuilder` and `VideoBuilder` already support a `PublishedAt` override, so
this needs no new fixture machinery. The 50 ms is removed and the ordering under
test becomes a property of the query rather than of the seeding speed.

### Remove the 2030 literal

```csharp
// Before — AdminGetAllSessionsEndpointV1Tests.cs:150
$"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&toDate=2030-01-01T00:00:00Z"
```

```csharp
// After — seed a known creation time and filter relative to it
var createdAt = new DateTimeOffset(2026, 3, 1, 12, 0, 0, TimeSpan.Zero);
SessionEntity session = await SeedAsync<IdentityDbContext, SessionEntity>(ctx =>
{
    SessionEntity entity = SessionFactory.CreateWithCreatedAt(TestUser.SuperAdminId, createdAt);
    ctx.Sessions.Add(entity);
    return entity;
});

var toDate = createdAt.AddDays(1).ToString("O");
var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=50&toDate={toDate}");
```

The filter now actually exercises the boundary rather than passing because the
boundary is four years away, and it will still be exercising it in 2031.

## The principle

**A test must be able to choose what time it is.** If the code under test reads the
clock itself, the test can only react to whatever the clock says, and every
temporal assertion becomes either a sleep, a tolerance, or a bet on the calendar.

Three rules follow:

1. **Time is a dependency, not an ambient fact.** Inject `TimeProvider` into
   services; pass the instant as a parameter to domain methods. `DateTime.UtcNow` in
   a method body is the same category of mistake as `Environment.GetEnvironmentVariable`
   in a method body (see
   [03-culture-and-environment-leakage.md](03-culture-and-environment-leakage.md)).
2. **Never sleep to make an assertion true.** `Task.Delay` and `Thread.Sleep` in a
   test are always a symptom of a missing seam. They convert a logic bug into a
   timing bug, which is strictly harder to diagnose.
3. **No expected value may be derived from the same clock the subject reads.** That
   is the tautology rule from
   [01-assertions-that-cannot-fail.md](01-assertions-that-cannot-fail.md) applied to
   time: `DateTimeOffset.UtcNow.Year + 1` in a test is the temporal equivalent of
   asking the localizer for the expected message.

## Checklist

- [ ] No `Task.Delay` or `Thread.Sleep` appears in a test body.
- [ ] Components that need the current time take `TimeProvider`; domain methods take
      the instant as a parameter.
- [ ] Tests that depend on the current time use `FakeTimeProvider` and set it
      explicitly.
- [ ] No expected value is computed from `DateTime.UtcNow` / `DateTimeOffset.UtcNow`
      in the same test that asserts against a subject reading the clock.
- [ ] `DateTime.Now` (local time) does not appear in `tests/`; use UTC.
- [ ] No assertion contains a hard-coded future date that will one day be in the
      past.
- [ ] Ordering in integration tests comes from seeded timestamps, not from the order
      or speed of seeding calls.
