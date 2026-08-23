# Integration testing standard

What a test under `tests/Integration/` must do. An integration test exists to prove that
code is **wired into the application** — that a request reaches a handler, that a handler
reaches a repository, that a repository reaches the right SQL. It is the only kind of test
that can catch dead code, and that is its most valuable job.

The evidence behind each rule is in the findings documents linked from each section.

## 1. Two legal entry points, and no third

Every integration test class inherits one of two bases. There is no other way in.

### Real HTTP — `BaseApiTest`

```csharp
[Collection("Database")]
public class AdminPublishArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task Publish_WithApprovedArticle_ShouldPersistPublishedStatus()
    {
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            var entity = ArticleFactory.CreateApproved(_categoryId);
            ctx.Articles.Add(entity);
            return entity;
        });

        HttpResponseMessage response = await Client
            .AsAdmin()
            .PatchAsync(ContentRoutes.Admin.PublishArticle(article.Id), content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext verify = CreateDbContext<ContentDbContext>();
        ArticleEntity persisted = await verify.Articles.SingleAsync(a => a.Id == article.Id);
        persisted.Status.Should().Be(EnumContentStatus.Published);
        persisted.PublishedAt.Should().NotBeNull();
    }
}
```

### Real repository from DI — `BaseRepositoryTest`

```csharp
[Collection("Database")]
public class ArticleRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task GetBySlugAsync_WithAPublishedArticle_ShouldReturnIt()
    {
        // arrange rows, then:
        var repo = Resolve<IArticleRepository>();

        ArticleEntity? found = await repo.GetBySlugAsync(article.Slug, CancellationToken.None);

        found.Should().NotBeNull();
        found!.Id.Should().Be(article.Id);
    }
}
```

Inside `tests/Integration/` it is forbidden to:

- `new` a validator, handler, specification, entity or error factory — that is a unit test
  in the wrong folder, and constructing it here turns green the exact metric that was
  warning you the code is not wired up
- mock a repository, service or `DbContext`
- use reflection to invoke private members
- build a `ServiceCollection` to assert DI registrations

There is no `Domain/` folder and no `Specifications/` folder under `tests/Integration/`.
Specifications are covered by calling the repository method that uses them, naming the
specification in the test's doc comment.

Folder paths mirror `src/`. Endpoint test files are named `<UseCase>EndpointV1Tests.cs`.

## 2. What the test host may replace, and what it must not

The test host is `ApiFixture`, a `WebApplicationFactory<Program>`. The rule is:

> **Replace outbound edges. Never replace composition.**

An outbound edge is something the application talks *to* that is outside the deployment
boundary. Composition is how the application assembles itself, and replacing it means the
test is exercising a different application from the one that ships.

| May be replaced | Must never be replaced |
| --- | --- |
| The database connection (Testcontainer) | Module registration and DI composition |
| Cloudinary (`ICloudinaryService`) | Configuration sources and their precedence |
| SMTP (`IEmailSender`) | Startup branches — anything reading the environment name |
| YouTube thumbnails, streaming-link resolution | Middleware order and the exception pipeline |
| | Authentication and authorization wiring |
| | `DbContext` pooling and lifetimes |
| | Background schedulers |

`ApiFixture` currently crosses that line in three places, and each is a finding:

**Pooling.** `ReplaceDbContext<T>` removes the pooled registration and re-adds a plain
`AddDbContext`:

```csharp
// tests/Integration/Common/Fixtures/ApiFixture.cs:120-130
var poolDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TDbContext));

if (poolDescriptor is not null)
{
    services.Remove(poolDescriptor);
}

services.AddDbContext<TDbContext>(options =>
{
    options.UseNpgsql(_db.ConnectionString).UseSnakeCaseNamingConvention();
});
```

The connection string must be replaced. The lifetime must not: pooled contexts are reset
between uses rather than constructed fresh, and any state a `DbContext` subclass holds in a
field behaves differently under the two registrations. Point the pooled registration at the
container instead. See [integration/04](../integration/04-production-wiring-divergence.md).

**Authentication.** `OverrideJwtAuthentication` (`ApiFixture.cs:137-156`) rewrites
`TokenValidationParameters` wholesale, and its own doc comment explains why: "the
production module captures env vars before test env vars are set". That is a startup-order
defect in `src/` being papered over in the test host, and its consequence is that **no test
anywhere authenticates with a token the application issued** — every token is minted by the
test with test parameters and validated against test parameters. Fix the capture order in
`src/`, then delete the override. See
[integration/03](../integration/03-authentication-contract-hole.md) and
[integration/02](../integration/02-environment-divergence.md).

**Background schedulers.** A live scheduler inside the test host mutates the same database
the tests assert against, on its own timer. It must be absent from the test host entirely,
and the jobs it would have run must be tested by invoking the job directly with a real
scope. See [integration/01](../integration/01-background-jobs-in-the-test-host.md).

Rate-limit relaxation is legitimate and is already modelled correctly: `DisableRateLimits`
defaults to `true`, and `RateLimitedApiFixture` overrides it to `false` for the tests that
assert real limiting behaviour. That is the right shape for any host-level opt-out — a
virtual switch with a fixture that turns it back on, not a permanent removal.

## 3. Assert persisted state, a response body, or both — never status alone

A `200 OK` says the pipeline did not throw. It says nothing about whether the command took
effect.

```csharp
// bad — passes if the handler's body is deleted, as long as it returns Ok()
response.StatusCode.Should().Be(HttpStatusCode.OK);

// good — status plus the side effect the endpoint exists to produce
response.StatusCode.Should().Be(HttpStatusCode.OK);

await using ContentDbContext verify = CreateDbContext<ContentDbContext>();
ArticleEntity persisted = await verify.Articles.SingleAsync(a => a.Id == article.Id);
persisted.Status.Should().Be(EnumContentStatus.Published);
```

For queries, the response body is the side effect and `ReadAsAsync<T>` deserializes into
the real production response record:

```csharp
AdminGetAllArticlesResponse body = await response.ReadAsAsync<AdminGetAllArticlesResponse>();

body.Articles.Should().HaveCount(3);
body.Articles.Select(a => a.Slug).Should().ContainInOrder("first", "second", "third");
```

Deserializing into the production record is what makes a renamed or dropped property a
build or test failure rather than a silent contract break.

## 4. Error responses assert status, title **and** detail

An error test that checks only the status code passes for any error the application can
produce at that status. There are usually several, and the one the test intends is rarely
the one it gets.

```csharp
// bad — passes for any 409 this endpoint can raise
response.StatusCode.Should().Be(HttpStatusCode.Conflict);

// also bad — the ProblemDetails shape is checked, the reason is not
await response.ShouldBeProblem(HttpStatusCode.Conflict);

// good — status, the exception type behind it, and the exact message the guard raised
await response.ShouldBeProblem<ConflictException>(
    HttpStatusCode.Conflict,
    Localized<ArticleErrorMessage>(m => m.AlreadyPublished())
);
```

The three pins do different jobs, and both of the last two are needed:

- **Status** is the transport contract. Several guards reach it.
- **`Title`** is `nameof(TException)`; every exception strategy sets it that way. The
  type argument is a compile-checked expectation, and it separates two exception
  *types* that share a status — four distinct types produce 403 — but not two guards
  inside one handler, which raise the same type.
- **`Detail`** is the localized message and is what tells two guards apart.

**Resolve the expected detail; never hardcode the sentence.**
`BaseApiTest.Localized<TMessage>` pulls the application's own `*ErrorMessage` class out
of the host container and invokes it under an explicit culture, so a `.resx` copy edit
moves both sides of the assertion together. This is not the self-comparison defect in
[03-assertion-catalogue.md](03-assertion-catalogue.md) entry 4: there both sides
resolved the same key through the same localizer, so nothing was proved; here the actual
value arrives over HTTP from the exception middleware, so the assertion proves the
pipeline picked the right guard *and* the right culture.

**The default request culture is `fr`, not `en`.** `LocalizationExtension` sets
`DefaultCulture = "fr"` with `AcceptLanguageHeaderRequestCultureProvider` as the only
provider (`src/Shared/Shared/Application/Extensions/LocalizationExtension.cs:17-22, 41`),
so a request that sends no `Accept-Language` header — which is every request
`BaseApiTest.Client` makes by default — is answered in French. A missing article yields
`Impossible de trouver l'article demandé.`, and only `Accept-Language: en` yields
`Could not find the requested article.`

Resolve the expected detail **in the culture the test's own request selects**:

```csharp
// the test sets no header, so the response is French and so is the expectation
Localized<SharedExceptionMessage>(m => m.EntityNotFound("Article"))

// the test sets Accept-Language: en, so the expectation must say so too
Localized<AuthenticationErrorMessage>(m => m.InvalidCredentials(), LocalizedMessage.EnglishCulture)
```

Getting this wrong does not fail loudly at the call site — it fails as a confusing
string mismatch, or passes while proving nothing where the two catalogues agree.

The empty-body escape hatch is closed: `ShouldBeProblem` fails on a missing body unless
`allowEmptyBody: true` is passed, and exactly one call site legitimately passes it, a
multipart model-binding failure the framework produces before the exception middleware
runs. A third `ShouldBeProblem(status, string)` overload exists and is `[Obsolete]` — it
substring-matches the detail and is a migration shim, not a form to write. See
[integration/08](../integration/08-assertion-escape-hatches.md) and
[specs/04](../specs/04-error-assertion-discipline.md).

## 5. Isolation is the framework's job, not the test author's

If a test has to remember to clean something up, some test will forget, and the failure
will land on a different test.

`BaseApiTest.InitializeAsync` does most of this correctly today — it resets the database,
invalidates the three in-process caches that outlive the reset, and seeds the well-known
users, all before every test method. The gap is the stubs, which are registered as
singletons on the host and therefore accumulate across the whole run:

```csharp
// tests/Integration/Workflows/EmailDeliveryFlowTests.cs:307-317
int alreadySent = stub.Sent.Count;
// ...
stub.Sent.Count.Should().BeGreaterThan(alreadySent);
```

The baseline capture is a workaround for state the framework should have cleared, and it
weakens the assertion from "this flow sent one email" to "the count went up". Elsewhere,
`StubStreamingLinkResolutionService.Reset()` is called by hand inside 13 individual test
methods — reliably, in those 13, and not at all in any test written next.

The rule:

- Every stub exposes a `Reset()`, and `BaseApiTest.InitializeAsync` calls all of them.
- No test calls `Reset()` itself, and no test captures a baseline to work around shared
  state.
- No `static` mutable field anywhere in `tests/Integration/`.
- Every scope is disposed. `CreateDbContext<T>`, `Resolve<T>` and
  `CreateScopedRepository<T, TDb>` on the base classes each call
  `Api.Services.CreateScope()` and drop the scope on the floor — roughly 1,189 undisposed
  scopes per run, each pinning its resolved services. Return the scope, or make the base
  class own and dispose them per test. See
  [integration/07](../integration/07-lifecycle-and-scope-leaks.md).

Both base classes carry `[Collection("Database")]`, which currently places **356 test
classes in a single xUnit collection** and serialises the entire integration suite against
a CI session timeout it is already approaching. The database container is the shared
resource, not the assembly; splitting into per-schema collections with independent
databases is the way out. See
[integration/06](../integration/06-parallelism-and-runtime.md).

## 6. Seeding is reconstitution, not behaviour

An arrangement represents rows that already existed. It must not re-run the business
events that originally created them, or every test's Arrange section fires welcome emails,
notification writes and promotion stamps into the assertion window.

`BaseApiTest` already handles this and the reasoning is worth preserving verbatim:

```csharp
// tests/Integration/Common/Base/BaseApiTest.cs:99-108
private static async Task SaveSeededAsync(DbContext context)
{
    context.ChangeTracker.DetectChanges();

    foreach (EntityEntry<IAggregate> entry in context.ChangeTracker.Entries<IAggregate>())
    {
        entry.Entity.ClearDomainEvents();
    }

    await context.SaveChangesAsync();
}
```

So: seed through `SeedAsync<TDbContext>(...)`, never by calling an endpoint to set up
another endpoint's precondition. Arranging via HTTP couples two contracts, doubles the
runtime, and turns a failure in the arrangement into a confusing failure in the assertion.

## 7. Determinism

- **No `Task.Delay` or `Thread.Sleep` in a test.** A sleep is a guess about timing that is
  wrong on a loaded CI worker. Wait on the observable condition, or remove the asynchrony
  from the test host.
- **Timestamps are explicit.** Never `DateTime.UtcNow` in an arrangement that a later
  assertion compares against — the two reads differ, and the tolerance that hides it also
  hides real drift.
- **Ordering is asserted only when the query orders.** If the endpoint has no `ORDER BY`,
  PostgreSQL may return rows in any order and the test is a coin flip. Assert
  `BeEquivalentTo` for unordered sets and `ContainInOrder` only against an explicit sort.
- **Counts are exact.** `Should().Be(5)` after seeding five rows into a reset database, not
  `BeGreaterThanOrEqualTo(5)`. There are 33 numeric `BeGreaterThanOrEqualTo` assertions in
  the suite and the arrangement fixes the count in almost all of them. See
  [03-assertion-catalogue.md](03-assertion-catalogue.md).
- **`BeOneOf` on a status code is not an assertion.** Eight sites accept two or three
  different status codes for one request; each of them is an unresolved question about the
  contract, written down as a passing test.

## Review checklist

- [ ] Class inherits `BaseApiTest` or `BaseRepositoryTest` and carries `[Collection("Database")]`
- [ ] Nothing `new`ed, mocked or reflected into inside `tests/Integration/`
- [ ] Only outbound edges replaced in the host — no composition, configuration, startup
      branch, scheduler or pooling change
- [ ] Assertion covers persisted state and/or a typed response body, not status alone
- [ ] Error tests use `ShouldBeProblem<TException>` with a detail resolved through
      `Localized<TMessage>`, in the culture that test's request selects — `fr` unless it
      sets `Accept-Language: en`
- [ ] No stub reset, no baseline capture, no static mutable state in the test
- [ ] Every DI scope disposed
- [ ] Seeded through `SeedAsync`, never through another endpoint
- [ ] No sleeps, exact counts, ordering asserted only where the query orders
