# Common Pitfalls & Lessons from Unit Tests

## Mistakes from Unit Tests to Avoid

The unit test suite (6100+ tests) uncovered patterns that caused issues. Integration tests must learn from these.

### 1. FluentAssertions vs AwesomeAssertions

**Wrong:**
```csharp
using FluentAssertions; // WRONG — this package is not installed
```

**Correct:**
```csharp
using AwesomeAssertions; // Correct — this is the assertion library used in this project
```

The API is identical but the package name differs. Always use `AwesomeAssertions`.

### 2. Action vs Func\<Task\> for Async Assertions

**Wrong — silently passes without executing the async code:**
```csharp
Action act = () => handler.Handle(command, CancellationToken.None);
act.Should().Throw<NotFoundException>(); // NEVER AWAITS!
```

**Correct:**
```csharp
Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);
await act.Should().ThrowAsync<NotFoundException>();
```

In integration tests, this manifests when asserting HTTP responses:
```csharp
// WRONG — don't wrap HTTP calls in Action
Action act = () => Client.GetAsync("/api/v1/nonexistent");

// CORRECT — just await the response and check status
HttpResponseMessage response = await Client.GetAsync("/api/v1/nonexistent");
response.StatusCode.Should().Be(HttpStatusCode.NotFound);
```

### 3. Times.Once (Property) vs Times.Once() (Method)

In unit tests, `Moq.Times.Once` is a property, not a method. Integration tests don't use Moq, but if you ever verify mock calls in integration test helpers:

```csharp
// WRONG
mock.Verify(x => x.Method(), Times.Once()); // compile error

// CORRECT
mock.Verify(x => x.Method(), Times.Once);
```

### 4. Missing CancellationToken in Async Mocks

Unit tests frequently forgot `It.IsAny<CancellationToken>()` in mock setups. Integration tests don't use mocks, but the lesson applies: always pass `CancellationToken` to async methods.

### 5. Navigation Property Null References

Unit tests crashed when handlers accessed `category.ContentType.Name` because the navigation property wasn't set on test entities. In integration tests, EF Core loads navigation properties via `Include()` — this problem doesn't exist if the repository correctly uses eager loading. Integration tests actually validate that eager loading works.

## Integration Test Specific Pitfalls

### 6. Port Conflicts

Testcontainers uses random ports, so port conflicts are rare. However, if you hardcode ports in test configuration, tests will fail when another process uses that port.

**Fix**: Always use `_container.GetConnectionString()` which includes the random port.

### 7. Docker Not Running

Testcontainers requires Docker. If Docker is not running, tests fail with a cryptic error.

**Fix**: Add a clear skip message:
```csharp
[Fact]
public async Task SomeTest()
{
    if (!DockerIsAvailable())
    {
        Assert.Skip("Docker is not available");
    }
    // ...
}
```

Or better, let it fail fast with a clear error in `PostgresFixture.InitializeAsync()`.

### 8. Test Ordering Dependencies

xUnit runs tests within a class in **undefined order** by default. If Test A inserts data that Test B depends on, tests become flaky.

**Fix**: Each test must be self-contained. Use `ResetAsync()` in `InitializeAsync()` and seed data within each test or in `SeedAsync()`.

### 9. Connection Pool Exhaustion

Each `CreateDbContext<T>()` call opens a new connection. If you create many contexts without disposing them, the connection pool fills up.

**Fix**: Always use `await using`:
```csharp
await using var context = CreateDbContext<ContentDbContext>();
```

### 10. Stale Environment Variables

`ApiFixture.SetEnvironmentVariables()` sets `Environment.SetEnvironmentVariable()` globally. If tests run in parallel across multiple collections, they share the same process and the same environment variables.

**Fix**: Use a single collection (`[Collection("Database")]`) for all integration tests. This ensures sequential execution within the same process. Alternatively, use `IConfiguration` overrides instead of environment variables.

### 11. Respawn and FK Ordering

Respawn handles FK cascades automatically, but if you have circular references or self-referencing FKs, it may fail.

**Fix**: This project's schema doesn't have circular FKs, so Respawn works out of the box. If you add circular references later, use `TablesToIgnore` to break the cycle.

### 12. EF Core Migration Version Drift

If you add a new migration to the source code but don't rebuild the integration test project, `MigrateAsync()` may fail because the test assembly has stale migration metadata.

**Fix**: Always rebuild before running integration tests:
```bash
dotnet build && dotnet test tests/Integration
```

### 13. JSON Serialization Mismatches

The API uses `JsonStringEnumConverter` for enum serialization. When deserializing responses in tests, use the same options:

```csharp
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    Converters = { new JsonStringEnumConverter() }
};

var body = await response.Content.ReadFromJsonAsync<MyResponse>(options);
```

Or better, configure `HttpClient` with the same serializer options via `WebApplicationFactory`.

### 14. Rate Limiting in Tests

Rate limiting middleware runs in integration tests. If a test class makes many requests, it may hit rate limits and get 429 responses unexpectedly.

**Fix options**:
- Increase rate limits for the Testing environment
- Disable rate limiting in `ApiFixture.ConfigureWebHost()`
- Test rate limiting explicitly in dedicated test classes

```csharp
// Disable rate limiting for most tests
builder.ConfigureServices(services =>
{
    // Remove rate limiter and add a no-op one
    // Or set very high limits for Testing environment
});
```

### 15. Slow First Test

The first test in a run is slow because:
1. Docker pulls the postgres image (first time only)
2. Container starts (~2-5 seconds)
3. Migrations run (~1-3 seconds)
4. Respawner initializes (~0.5 seconds)

Subsequent tests in the same collection are fast (Respawn reset ~50ms).

**Fix**: This is expected. The total overhead is 5-10 seconds for the first test, amortized across all tests in the collection.

## Checklist Before Writing a Test

- [ ] Does this test need a real database? (If not, write a unit test instead)
- [ ] Does this test need the HTTP pipeline? (If not, write a repository test instead)
- [ ] Is the data I need seeded in `SeedAsync()` or inline?
- [ ] Am I using `await using` for all DbContext instances?
- [ ] Am I using separate DbContexts for arrange and act?
- [ ] Am I asserting the HTTP status code before reading the response body?
- [ ] Am I using `AwesomeAssertions` (not FluentAssertions)?
- [ ] Does the test work when run in isolation (not depending on other tests)?
