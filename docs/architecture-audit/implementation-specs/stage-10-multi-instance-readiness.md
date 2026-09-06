# Stage 10 — Multi-instance readiness

Closes **[04 §8]** (Critical), **[04 §9]** (High), **[04 §10]** (High), **[04 §14]** (Medium),
**[01 §1.12]**, **[01 §1.13]**, **[01 §1.15]**. Today the application is only correct as a single
process:

- The popular-feed caches are `IMemoryCache` entries evicted through an in-process
  `CancellationTokenSource` (`CacheInvalidator.Invalidate()` cancels a token only *this* process
  holds). A like handled on instance A leaves instance B serving the stale feed for the full TTL.
- Four Quartz jobs (`ExpiredOtpCleanupJob`, `AbandonedDraftCleanupJob`,
  `ShortVideoViewEventCleanupJob`, `OutboxEmailDispatcherJob`) are registered with the in-memory
  store — two instances run every job twice, concurrently.
- `ContentModule` runs `ContentTypeSeeder.SeedAllAsync().GetAwaiter().GetResult()` during
  registration; two cold starts race the `AnyAsync` check and double-insert. Worse, the
  short-circuit (`if (alreadySeeded) return;`) means a database seeded before `Lyrics` was added
  to the list **never receives it**.
- `UseMigration<TContext>()` calls `Database.MigrateAsync()` inside pipeline construction — two
  instances race the migration lock, and Stage 9's `CREATE INDEX CONCURRENTLY` cannot run there
  at all.
- The three `AddDbContextPool` registrations configure no retry, no command timeout, no pool cap.

> Draft — finalized against the tree Stage 9 lands on. All types below verified against the
> current tree.

---

## Decisions

| # | Question | Options weighed | Decision |
| --- | --- | --- | --- |
| D1 | Cross-instance invalidation | Redis pub/sub eviction, or version-key composition | **Version keys.** The eviction-token pattern cannot be distributed (a `CancellationTokenSource` is process-local by nature). A version key makes invalidation a Redis `INCR` and turns "is this entry stale" into cache-key composition — no pub/sub, no fan-out, works with plain `IDistributedCache`. |
| D2 | Keep `IMemoryCache` too? | Redis-only, or L1 memory + L2 Redis | **Redis-only for the popular feeds.** The entries are small DTO lists with a 10-minute TTL; the L1 layer is what created the coherence bug. `HybridCache` can come later if latency demands it. |
| D3 | Job coordination | Quartz JDBC clustering, or advisory locks per job | **Quartz clustering.** The jobs are already Quartz (`AddScheduledJob<TJob>` → `AddQuartz`); pointing the scheduler at a persistent clustered store fixes all four at the registration seam, with none of the per-job lock code an advisory-lock approach needs. |
| D4 | Seeding | keep registration-time seeding + lock, or a hosted service | **Hosted service + advisory lock + per-item idempotency.** Seeding during `RegisterModule` blocks DI construction on I/O and cannot be ordered after migrations move. `IHostedService` runs after the host is built; `pg_advisory_lock` serializes replicas; upsert-per-row replaces the all-or-nothing `AnyAsync` short-circuit so the missing `Lyrics` row heals itself. |
| D5 | Dead seeder infra | wire `IDataSeeder`/`UseSeed`, or delete it | **Wire it.** The hosted service enumerates `IEnumerable<IDataSeeder>` — the interface finally earns its registration, and `ContentTypeSeeder` implements it instead of being manually resolved. `UseSeed()` and `SeedDataAsync` are deleted. |
| D6 | Migrations | init-container/CLI step, or keep startup migration behind a lock | **Explicit step.** `dotnet run --project src/Api -- migrate` (and the compose/deploy pipeline calls it) applies and exits. `EnableMigrations` defaults to `false` outside Development. Startup migration is what blocks Stage 9's `CONCURRENTLY` indexes and what makes deploys apply 60+ destructive operations implicitly `[04 §10]`. |

---

## Checklist

- [ ] 10.1 — Redis in compose/CI; `AddStackExchangeRedisCache`; Testcontainers Redis in the fixture
- [ ] 10.2 — Version-key cache: `IVersionedCache` + the three invalidators become `INCR`s
- [ ] 10.3 — Quartz JDBC store, clustered, `[DisallowConcurrentExecution]` on the four jobs
- [ ] 10.4 — Seeding: `IDataSeeder` wired, advisory-locked hosted service, per-row idempotent upserts (heals `Lyrics`)
- [ ] 10.5 — `migrate` entry point; `EnableMigrations=false` outside Development; `UseMigration` deleted
- [ ] 10.6 — `EnableRetryOnFailure` + `CommandTimeout(30)` + pool size on `ConfigureDbContextOptions`
- [ ] 10.7 — Dockerfile copies the Mailer projects
- [ ] 10.8 — Tests: invalidation visible across two cache clients; seeder run twice inserts once
- [ ] 10.9 — Verify (build 0/0, csharpier, unit, integration; `docker build` succeeds)

---

## Part A — Distributed cache

### 10.1–10.2 Version-keyed Redis cache

The three invalidators (`PopularArticlesCacheInvalidator`, `PopularVideosCacheInvalidator`,
`PopularTagsCacheInvalidator`) are empty subclasses of `CacheInvalidator` whose only job is owning
a distinct eviction token. The interface changes shape — `GetEvictionToken()` cannot exist in a
distributed world:

```csharp
namespace _116.Content.Application.Shared.Cache;

/// <summary>
/// Invalidates one named cache region by advancing its version. Entries written under an older
/// version become unreachable immediately, on every instance.
/// </summary>
public interface ICacheInvalidator
{
    /// <summary>
    /// Advances the region's version, orphaning every entry composed with the previous one.
    /// </summary>
    Task InvalidateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// The current version, composed into every cache key for this region.
    /// </summary>
    Task<long> GetVersionAsync(CancellationToken cancellationToken = default);
}
```

```csharp
namespace _116.Content.Infrastructure.Cache;

/// <summary>
/// Version-key <see cref="ICacheInvalidator" /> over <see cref="IDistributedCache" />. The
/// version lives in Redis, so an invalidation on one instance is visible to all of them; the
/// orphaned entries age out through their absolute expiration.
/// </summary>
public abstract class CacheInvalidator(IDistributedCache cache) : ICacheInvalidator
{
    /// <summary>
    /// The region name separating this invalidator's version from the other regions'.
    /// </summary>
    protected abstract string Region { get; }

    private string VersionKey => $"cache-version:{Region}";

    /// <inheritdoc />
    public async Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        long next = await GetVersionAsync(cancellationToken) + 1;
        await cache.SetStringAsync(VersionKey, $"{next}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<long> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        string? raw = await cache.GetStringAsync(VersionKey, cancellationToken);
        return raw is null ? 0 : long.Parse(raw);
    }
}
```

> `INCR` via `IDistributedCache` is read-then-write and can lose a concurrent bump; that is
> acceptable here because *any* bump invalidates (two racing bumps that collapse into one still
> orphan the old version). If exactness is wanted, drop to `IConnectionMultiplexer` and
> `StringIncrementAsync` — decide at finalization by whether StackExchange.Redis is referenced
> directly anyway.

`PublicGetPopularArticlesHandler` composes the version into its key and swaps `IMemoryCache` for
`IDistributedCache` (`PublicGetPopularVideosHandler` and the tags handlers are identical in
shape):

```csharp
long version = await cacheInvalidator.GetVersionAsync(cancellationToken);
string categoryPart = query.CategoryId?.ToString() ?? "all";
string excludePart = query.ExcludeId?.ToString() ?? "none";
string cacheKey = $"popular_articles:v{version}:{query.Limit}_{categoryPart}_{excludePart}";

byte[]? cachedBytes = await cache.GetAsync(cacheKey, cancellationToken);
if (cachedBytes is not null)
{
    return new PublicGetPopularArticlesResult(
        Articles: JsonSerializer.Deserialize<IReadOnlyList<ArticleSummaryDto>>(cachedBytes)!
    );
}

// …existing repository + mapper calls unchanged…

await cache.SetAsync(
    cacheKey,
    JsonSerializer.SerializeToUtf8Bytes(dtoList),
    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl },
    cancellationToken
);
```

The Stage 8 engagement handlers call `cacheInvalidator.Invalidate()` fire-and-forget today; they
become `await cacheInvalidator.InvalidateAsync(cancellationToken)` — their unit tests
(`VerifyInvalidateCalled`) follow the signature.

Registration (`Program.cs`):

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "116:";
});
```

## Part B — Jobs and seeding

### 10.3 Clustered Quartz

`QuartzExtension.AddScheduledJob<TJob>` keeps its signature; the store configuration moves into
the first-call setup:

```csharp
services.AddQuartz(q =>
{
    q.UsePersistentStore(store =>
    {
        store.UsePostgres(postgres =>
        {
            postgres.ConnectionString = connectionString;
            postgres.TablePrefix = "quartz.qrtz_";
        });
        store.UseClustering();
        store.UseNewtonsoftJsonSerializer();
    });

    var jobKey = new JobKey(typeof(TJob).Name);
    q.AddJob<TJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts =>
        opts.ForJob(jobKey).WithIdentity($"{typeof(TJob).Name}-trigger").WithCronSchedule(cronExpression)
    );
});
```

The four job classes get `[DisallowConcurrentExecution]` so a slow run and its successor cannot
overlap even on one instance. The Quartz schema ships as a migration in `quartz` schema
(generated, left unapplied per house rule).

### 10.4 Seeding

`ContentTypeSeeder.SeedAllAsync` today short-circuits on `AnyAsync` — which is both the race and
the reason `Lyrics` never seeds into an old database. It becomes an `IDataSeeder` with per-row
idempotency:

```csharp
/// <inheritdoc />
public async Task SeedAsync(CancellationToken cancellationToken = default)
{
    foreach (string name in ContentTypeNames)
    {
        bool exists = await context.ContentTypes.AnyAsync(t => t.Name == name, cancellationToken);
        if (exists)
        {
            continue;
        }

        context.ContentTypes.Add(ContentTypeEntity.Create(name: name));
    }

    await context.SaveChangesAsync(cancellationToken);
}
```

One hosted service replaces every registration-time `GetAwaiter().GetResult()` seeding call
(`ContentModule.cs:269` and its Identity siblings):

```csharp
namespace _116.Shared.Infrastructure.Seeding;

/// <summary>
/// Runs every registered <see cref="IDataSeeder" /> once per deployment under a Postgres
/// advisory lock, so N starting replicas seed exactly once between them.
/// </summary>
public class DataSeedingHostedService(IServiceProvider serviceProvider, ILogger<DataSeedingHostedService> logger)
    : IHostedService
{
    private const long SeedLockKey = 116_001;

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        var connection = new NpgsqlConnection(scope.ServiceProvider.GetRequiredService<IConfiguration>()
            .GetConnectionString("Database"));
        await connection.OpenAsync(cancellationToken);

        await using (NpgsqlCommand acquire = new($"SELECT pg_advisory_lock({SeedLockKey})", connection))
        {
            await acquire.ExecuteNonQueryAsync(cancellationToken);
        }

        try
        {
            foreach (IDataSeeder seeder in scope.ServiceProvider.GetServices<IDataSeeder>())
            {
                logger.LogInformation("Seeding via {Seeder}.", seeder.GetType().Name);
                await seeder.SeedAsync(cancellationToken);
            }
        }
        finally
        {
            await using NpgsqlCommand release = new($"SELECT pg_advisory_unlock({SeedLockKey})", connection);
            await release.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

`UseSeed()`/`SeedDataAsync` in `ApplicationBuilderExtension` are deleted `[01 §1.13]`.

### 10.5 Migrations out of the pipeline

`UseMigration<TContext>()` (which does `MigrateDatabaseAsync(...).GetAwaiter().GetResult()`
inside pipeline construction) is deleted. `Program.cs` grows an explicit mode before
`builder.Build()`'s pipeline is wired:

```csharp
if (args.Contains("migrate"))
{
    using IServiceScope scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<CoreDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<ContentDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<MailerDbContext>().Database.MigrateAsync();
    return;
}
```

`ModuleOptions.EnableMigrations` defaults to `false`; Development opts in explicitly so the local
loop keeps working. Deploy order becomes: run `migrate` (as a job/init container), then roll the
instances — which is also the only order under which Stage 9's `CONCURRENTLY` indexes can build.

## Part C — Resilience and the build

### 10.6 EF resilience

`BaseModule.ConfigureDbContextOptions` (the single seam all three pooled contexts share):

```csharp
options
    .UseNpgsql(connectionString, npgsql =>
    {
        npgsql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
        npgsql.CommandTimeout(30);
    })
    .UseSnakeCaseNamingConvention();
```

> `EnableRetryOnFailure` installs a retrying execution strategy: code that opens explicit
> transactions must wrap them in `CreateExecutionStrategy().ExecuteAsync(...)`. Nothing in
> Content/Core opens one today (`[04 §7]` — that's the Stage 13 work), so land this **before**
> Stage 13 and let 13 build on the strategy, not fight it.

### 10.7 Dockerfile

The build stage copies every module's projects but Mailer's — the container restore fails from a
clean context `[01 §1.15]`. Add the missing pair alongside the existing module copies:

```dockerfile
COPY ["src/Modules/Mailer/Mailer/Mailer.csproj", "src/Modules/Mailer/Mailer/"]
COPY ["src/Modules/Mailer/Mailer.Contracts/Mailer.Contracts.csproj", "src/Modules/Mailer/Mailer.Contracts/"]
```

(Exact project list from the current `.sln` at implementation time.)

---

## Tests

- **Integration — cache coherence:** two `IDistributedCache` clients over one Testcontainers
  Redis; write through client A's handler path, `InvalidateAsync` via B, assert A's next read
  misses. Plus the end-to-end regression: like an article over HTTP, popular feed no longer
  serves the stale count (exists today in `CacheInvalidationRegressionTests` — it must survive
  the Redis swap unchanged).
- **Integration — seeder idempotency:** run `DataSeedingHostedService.StartAsync` twice against
  one database; content-type rows count once. Delete the `Lyrics` row, run again, assert it
  heals.
- **Integration — jobs:** `[DisallowConcurrentExecution]` present on all four job classes
  (reflection assertion), Quartz store config points at Postgres.
- **Unit:** the version-key invalidator (bump orphans the old key; region isolation between the
  three invalidators).

---

## Rollout

Redis becomes a hard runtime dependency: compose, CI, and production provisioning land **first**.
Deploy order changes permanently: `migrate` step, then instances. First deploy after this stage
runs the Quartz schema migration.

---

## Verification

1. `dotnet build --no-incremental` — 0 warnings, 0 errors; `dotnet csharpier check .`.
2. Unit + integration green (Redis Testcontainer in the fixture).
3. `grep -rn "IMemoryCache" src/Modules/Content` → nothing (the popular feeds were its only Content consumers).
4. `grep -rn "GetAwaiter().GetResult()" src/Modules src/Shared` → no seeding/migration call sites left.
5. `docker build .` succeeds from a clean context.
6. Two local instances + one Redis: like on :5025, feed on :5026 reflects it within one request.

---

**PR:** `fix(infra): make the app safe to run on more than one instance`
