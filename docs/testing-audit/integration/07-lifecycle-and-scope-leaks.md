# Medium — About 1,189 DI scopes are created and never disposed

Three helper methods on the integration base classes each create an `IServiceScope`
and drop the reference before returning. The scopes are rooted in a container that
lives for the whole process, so nothing collects them, and the services they hold —
including Npgsql connections — are never released. The suite calls those helpers
about 1,189 times per run. Three other methods in the same file do it correctly,
which shows the pattern is understood; it just was not applied where it mattered
most.

## The problem

```csharp
// tests/Integration/Common/Base/BaseApiTest.cs:49-54
protected TDbContext CreateDbContext<TDbContext>()
    where TDbContext : DbContext
{
    var scope = Api.Services.CreateScope();
    return scope.ServiceProvider.GetRequiredService<TDbContext>();
}
```

```csharp
// tests/Integration/Common/Base/BaseRepositoryTest.cs:34-64
protected TDbContext CreateDbContext<TDbContext>()
    where TDbContext : DbContext
{
    var scope = Api.Services.CreateScope();
    return scope.ServiceProvider.GetRequiredService<TDbContext>();
}

protected TService Resolve<TService>()
    where TService : notnull
{
    var scope = Api.Services.CreateScope();
    return scope.ServiceProvider.GetRequiredService<TService>();
}

protected (TRepository Repo, TDbContext Db) CreateScopedRepository<TRepository, TDbContext>()
    where TRepository : notnull
    where TDbContext : DbContext
{
    var scope = Api.Services.CreateScope();
    return (
        scope.ServiceProvider.GetRequiredService<TRepository>(),
        scope.ServiceProvider.GetRequiredService<TDbContext>()
    );
}
```

In each case `scope` is a local that goes out of scope at the `return`. The
`IServiceScope` is never disposed, and because `Api.Services` is the root provider
of a `WebApplicationFactory` that lives for the entire test session, the scope stays
reachable from the container's disposable-tracking list for the whole run.

Measured call sites outside `tests/Integration/Common/Base/`:

| Helper | Call sites |
| --- | --- |
| `CreateDbContext<` | 838 |
| `Resolve<` | 287 |
| `CreateScopedRepository<` | 64 |
| **Total** | **1,189** |

Three methods in the same file get it right, and they are the proof that the fix is
already understood here:

```csharp
// tests/Integration/Common/Base/BaseApiTest.cs:139, 152, 165
using var scope = Api.Services.CreateScope();
```

## Why it matters

**The `await using` at call sites does not help.** The common idiom is:

```csharp
// tests/Integration/Workflows/AuthenticationFlowTests.cs:39
await using IdentityDbContext verifyContext = CreateDbContext<IdentityDbContext>();
```

That disposes the `DbContext`, which does return its Npgsql connection to the pool.
What it does not dispose is the scope that produced it, along with every other
scoped service that scope resolved on the way — the `ICurrentActor`, the repository
graph, the unit of work. Those stay alive, attached to the root provider, for the
rest of the session.

**The `Resolve` and `CreateScopedRepository` paths dispose nothing at all.** A call
like `Resolve<IArticleRepository>()` returns a repository whose injected `DbContext`
is held only by the abandoned scope. No caller can dispose it, because no caller
ever sees it. That context holds a pooled Npgsql connection until the scope is
collected, which is never.

The container is `postgres:16-alpine`, whose default `max_connections` is 100. The
suite has not hit that ceiling — it is serial today ([06](06-parallelism-and-runtime.md)),
so contexts are acquired and abandoned one at a time and the Npgsql pool reclaims
some through its own idle handling. That is luck, not design, and it is exactly the
luck that runs out in the parallelisation work described in
[06](06-parallelism-and-runtime.md): four collections running concurrently, each
leaking scopes at the same rate, is the shape that produces
`FATAL: sorry, too many clients already` on CI and nowhere else.

**It also blocks a fix in another document.**
[04-production-wiring-divergence.md](04-production-wiring-divergence.md) recommends
restoring `AddDbContextPool` to match production. A pooled context returns to the
pool when its scope is disposed. Restoring pooling while 1,189 scopes are abandoned
would mean pooled contexts are checked out and never returned — turning a wiring fix
into a resource exhaustion bug. **This document must land before that one.**

Finally, memory. Every abandoned scope retains its resolved services, its change
tracker, and every entity graph those trackers loaded. Over 1,879 tests that is
real, monotonically growing heap in a process that already has a
`TestSessionTimeout` it is racing.

## The fix

Track the scopes on the test instance and dispose them when the test ends. Both base
classes already implement `IAsyncLifetime`, so the hook exists.

```csharp
// tests/Integration/Common/Base/BaseRepositoryTest.cs — after
[Collection("Database")]
public abstract class BaseRepositoryTest : IAsyncLifetime
{
    /// <summary>
    /// Every scope opened by <see cref="CreateDbContext{TDbContext}" />, <see cref="Resolve{TService}" />
    /// and <see cref="CreateScopedRepository{TRepository, TDbContext}" />. The application container
    /// outlives the whole run, so a scope that is not disposed here is never disposed at all and
    /// holds its Npgsql connection until the process exits.
    /// </summary>
    private readonly List<IServiceScope> _scopes = [];

    /// <summary>
    /// Opens a scope, records it for disposal at the end of the test, and returns it.
    /// </summary>
    private IServiceScope OpenScope()
    {
        IServiceScope scope = Api.Services.CreateScope();
        _scopes.Add(scope);
        return scope;
    }

    /// <inheritdoc cref="CreateDbContext{TDbContext}" />
    protected TDbContext CreateDbContext<TDbContext>()
        where TDbContext : DbContext => OpenScope().ServiceProvider.GetRequiredService<TDbContext>();

    /// <inheritdoc cref="Resolve{TService}" />
    protected TService Resolve<TService>()
        where TService : notnull => OpenScope().ServiceProvider.GetRequiredService<TService>();

    /// <inheritdoc cref="CreateScopedRepository{TRepository, TDbContext}" />
    protected (TRepository Repo, TDbContext Db) CreateScopedRepository<TRepository, TDbContext>()
        where TRepository : notnull
        where TDbContext : DbContext
    {
        IServiceProvider provider = OpenScope().ServiceProvider;
        return (provider.GetRequiredService<TRepository>(), provider.GetRequiredService<TDbContext>());
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        foreach (IServiceScope scope in _scopes)
        {
            if (scope is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else
            {
                scope.Dispose();
            }
        }

        _scopes.Clear();
    }
}
```

`BaseApiTest` takes the same `_scopes` list and `OpenScope` helper, and its existing
`DisposeAsync` gains the loop alongside `Client.Dispose()`.

Three properties make this safe:

- **A plain `List<T>` is sufficient.** Both base classes are per-test-instance and
  every class is in a collection whose tests run serially, so no two threads add to
  the same list. If [06](06-parallelism-and-runtime.md) shards collections, that
  stays true — sharding parallelises *across* collections, not within one.
- **Disposing a scope after its `DbContext` was already disposed is a no-op.** The
  838 call sites that write `await using var context = CreateDbContext<T>()` keep
  working unchanged; double disposal of a `DbContext` is defined to be safe.
- **No call site changes.** All 1,189 benefit from an edit to two files.

`Api.Services.CreateScope()` returns an `AsyncServiceScope` in modern
`Microsoft.Extensions.DependencyInjection`, which implements both `IDisposable` and
`IAsyncDisposable`; preferring the async path lets `DbContext` release its
connection asynchronously rather than blocking.

## The principle

**A method that opens a resource must either close it or hand it to something that
will.** A helper that returns a service resolved from a scope it then forgets has
made disposal impossible for every caller — no amount of care at the call site can
recover it, which is why 838 correctly written `await using` statements do not fix
this.

The second principle is about where to put the fix: **when a leak has a thousand
call sites, the fix belongs in the one place they all pass through.** The
alternative — asking every author to remember a pattern — is the same failure mode
as the manual stub resets in [05](05-shared-mutable-state.md), and it fails the same
way.

## Checklist

- [ ] `_scopes` list and `OpenScope()` added to `BaseApiTest` and `BaseRepositoryTest`
- [ ] `CreateDbContext`, `Resolve` and `CreateScopedRepository` route through `OpenScope()`
- [ ] Both `DisposeAsync` implementations dispose every tracked scope, preferring
      `IAsyncDisposable`
- [ ] The three existing correct sites (`BaseApiTest.cs:139, 152, 165`) left as
      `using var scope` — they are already right
- [ ] Landed **before** the `AddDbContextPool` change in
      [04](04-production-wiring-divergence.md)
- [ ] Peak connection count against the container measured before and after
