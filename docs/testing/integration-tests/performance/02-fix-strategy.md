# Fix Strategy: Shared ApiFixture via Collection Fixture

## Chosen Approach

**Promote `ApiFixture` to a collection-level fixture**, shared across all 45 test classes.
Instead of each class creating its own `WebApplicationFactory<Program>`, the collection
creates exactly one instance that all classes reuse.

This is the standard xUnit pattern for expensive shared resources and is explicitly
recommended by the ASP.NET Core documentation for integration testing.

## What Changes

### Before (Current)

```
PostgresFixture  ──  1 instance (collection fixture)  ✓ correct
ApiFixture       ── 45 instances (per-class)           ✗ wasteful
HttpClient       ── 23 instances (per API test class)  ✗ wasteful
```

### After (Fixed)

```
PostgresFixture  ──  1 instance (collection fixture)   ✓
ApiFixture       ──  1 instance (collection fixture)   ✓ shared
HttpClient       ── per-test (from shared ApiFixture)  ✓ lightweight
```

## Implementation Details

### Step 1: Update DatabaseCollection to include ApiFixture

```csharp
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<PostgresFixture>, ICollectionFixture<ApiFixture>;
```

Wait — `ApiFixture` depends on `PostgresFixture` (constructor injection). xUnit v3
`ICollectionFixture` supports constructor injection between fixtures in the same collection.
So `ApiFixture` will receive the already-initialized `PostgresFixture`.

### Step 2: Make ApiFixture implement IAsyncLifetime

`ApiFixture` needs to set environment variables and configure services on first creation,
not just through `ConfigureWebHost`. Since `WebApplicationFactory` is lazy (it starts
the server on first `CreateClient()` or `Services` access), this mostly just works.

However, we do need `ApiFixture` to implement `IAsyncLifetime` so that it's properly
disposed at end of the collection's lifetime (not per-class).

### Step 3: Update BaseApiTest and BaseRepositoryTest

Both base classes change from creating their own `ApiFixture` to accepting one:

```csharp
protected BaseApiTest(PostgresFixture db, ApiFixture api)
{
    Db = db;
    Api = api;           // Shared, not new
    Client = api.CreateClient();
}
```

### Step 4: Update all 45 test class constructors

Each test class needs to accept both fixtures:

```csharp
// Before
public class SmokeTest(PostgresFixture db) : BaseApiTest(db)

// After
public class SmokeTest(PostgresFixture db, ApiFixture api) : BaseApiTest(db, api)
```

### Step 5: Remove ApiFixture disposal from base test classes

Since `ApiFixture` is now collection-scoped, `DisposeAsync()` in the base classes must
NOT dispose it. The collection fixture handles disposal at the end of the test run.

`HttpClient` is still per-test and should still be disposed.

## Expected Performance Improvement

| Metric | Before | After | Improvement |
|--------|--------|-------|-------------|
| WebApplicationFactory startups | 45 | 1 | ~44x fewer |
| Startup overhead | 90-225s | 2-5s | ~45x faster |
| Total test time (estimated) | ~360s | ~30-60s | ~6-12x faster |
| Per-test average | ~670ms | ~55-110ms | ~6-12x faster |

## Risk Assessment

| Risk | Mitigation |
|------|------------|
| Shared server state leaks between tests | Respawn resets DB per-test; DI scopes are per-request |
| Environment variable race conditions | Already set once; no concurrent modification |
| Test isolation | Each test still gets fresh DB state via Respawn |
| HttpClient shared state | Each test creates its own HttpClient from the shared factory |

## Files Modified

| File | Change |
|------|--------|
| `PostgresFixture.cs` | Update `CollectionDefinition` to include `ApiFixture` |
| `ApiFixture.cs` | Add `IAsyncLifetime`, move env var setup |
| `BaseApiTest.cs` | Accept `ApiFixture` parameter, stop creating/disposing it |
| `BaseRepositoryTest.cs` | Accept `ApiFixture` parameter, stop creating/disposing it |
| 45 test class files | Update constructor to pass both fixtures |

## Alternative Approaches Considered

### Multiple Collections with Separate Databases
- Pros: enables parallelism
- Cons: requires multiple Testcontainers, complex setup, higher resource usage
- Verdict: overkill when the main problem is redundant WebApplicationFactory startups

### TestServer Caching in Base Class (static)
- Pros: simple static field approach
- Cons: fights against xUnit's lifecycle model, thread-safety concerns
- Verdict: xUnit's built-in collection fixtures are the idiomatic solution

### Reducing Respawn to per-class instead of per-test
- Pros: fewer truncate calls
- Cons: tests within a class would share state, reducing isolation
- Verdict: not worth the isolation loss for ~5% improvement
