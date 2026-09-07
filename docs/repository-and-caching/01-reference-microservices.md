# Reference: `dotnet-microservices`

Path: `/Users/coolbeatz/projects/116/116/dotnet-microservices`

## Repository layering

Four levels, each adding exactly one thing.

**Level 1 — the contract, in `BuildingBlocks`.** Note the last two members: the repository does
not try to own every query shape, it exposes `IQueryable` and lets callers compose.

```csharp
// src/BuildingBlocks/Blocks.EntityFrameworkCore/Repositories/IRepository.cs
public interface IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);
    Task<bool> ExistsAsync(TKey id, CancellationToken ct = default);
    Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default);
    TEntity Update(TEntity entity);
    void Remove(TEntity entity);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    void ClearTracking();

    IQueryable<TEntity> Query();
    IQueryable<TEntity> QueryNotTracked();
}
```

**Level 2 — the generic implementation.** Every member is `virtual`; `Query()` is the seam.

```csharp
public abstract class RepositoryBase<TContext, TEntity, TKey> : IRepository<TEntity, TKey>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    public virtual IQueryable<TEntity> Query() => _entity;
    public virtual IQueryable<TEntity> QueryNotTracked() => _entity.AsNoTracking();

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
        => await Query().SingleOrDefaultAsync(e => e.Id.Equals(id), ct);
}
```

**Level 3 — per-service closure of the `TContext` generic.** One line, kills the type noise at
every call site below it.

```csharp
// src/Services/Review/Review.Persistence/Repositories/Repository.cs
public class Repository<TEntity>(ReviewDbContext dbContext)
    : RepositoryBase<ReviewDbContext, TEntity>(dbContext)
        where TEntity : class, IEntity<int>;
```

**Level 4 — the entity repository.** This is the piece worth stealing:

```csharp
// src/Services/Review/Review.Persistence/Repositories/ReviewerRepository.cs
public class ReviewerRepository(ReviewDbContext dbContext) : Repository<Reviewer>(dbContext)
{
    public override IQueryable<Reviewer> Query()
    {
        return base.Entity.Include(e => e.Specializations);
    }

    public async Task<Reviewer?> GetByUserIdAsync(int userId, CancellationToken ct = default)
        => await Query().SingleOrDefaultAsync(e => e.UserId == userId, ct);

    public async Task<Reviewer?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await Query().SingleOrDefaultAsync(e => e.Email.Value.ToLower() == email.ToLower(), ct);
}
```

**Why this matters.** The `Include` graph for an aggregate is declared **once**, in the `Query()`
override, and every finder inherits it. Add an `Include` and every read of that aggregate picks
it up; there is no possibility of one finder returning a hydrated aggregate and another returning
a half-loaded one.

## Caching

Caching is welded in by **inheritance**, not decoration. There is no Scrutor reference anywhere in
the repository and zero `Decorate` calls.

```csharp
// src/BuildingBlocks/Blocks.EntityFrameworkCore/Repositories/CachedRepository.cs
public abstract class CachedRepository<TDbContext, TEntity, TId>(TDbContext _dbContext, IMemoryCache _cache)
    where TDbContext : DbContext
    where TEntity : class, IEntity<TId>, ICacheable
    where TId : struct
{
    public IEnumerable<TEntity> GetAll()
        => _cache.GetOrCreateByType(entry => _dbContext.Set<TEntity>().AsNoTracking().ToList());

    public TEntity GetById(TId id) => GetAll().Single(e => e.Id.Equals(id));
}

// src/Services/Review/Review.Persistence/Repositories/AssetTypeDefinitionRepository.cs
public class AssetTypeDefinitionRepository(ReviewDbContext dbContext, IMemoryCache cache)
    : CachedRepository<ReviewDbContext, AssetTypeDefinition, AssetType>(dbContext, cache);
```

Four decisions make that safe, and all four are load-bearing:

**1. Only `ICacheable` types participate.** Every implementer is reference data — `AssetTypeDefinition`,
`ArticleStageTransition`, `AssetStateTransition`, `TimelineTemplate`, `TimelineVisibility`. Enum-shaped
metadata that changes with a migration, never at runtime.

**2. The key carries no parameters.** One entry per entity type, holding the whole table:

```csharp
public static T GetOrCreateByType<T>(this IMemoryCache memoryCache, Func<ICacheEntry, T> factory)
    => memoryCache.GetOrCreate(typeof(T).FullName!, factory)!;
```

**3. `GetById` never touches the database.** It is `Single()` over the in-memory list. For a lookup
table of 20 rows read on every request, that removes the query entirely rather than making it faster.

**4. Detached entities are fenced off from the change tracker.** Every `SaveChangesAsync` demotes
`ICacheable` entries back to `Unchanged`, so a cached instance that wanders into the tracker cannot
be written back:

```csharp
// src/BuildingBlocks/Blocks.EntityFrameworkCore/Extensions/DbContextExtensions.cs
public static void UnTrackCacheableEntities(this DbContext context)
{
    foreach (var entry in context.ChangeTracker.Entries())
    {
        if (entry.Entity is ICacheable)
            entry.State = EntityState.Unchanged;
    }
}

// src/Services/Review/Review.Persistence/ReviewDbContext.cs:38
public async override Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    this.UnTrackCacheableEntities();
    return await base.SaveChangesAsync(ct);
}
```

The cache is warmed at boot so the first request never pays for the fill:

```csharp
// src/Services/Review/Review.Persistence/DatabaseCacheLoader.cs
public class DatabaseCacheLoader(IServiceProvider _serviceProvider) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReviewDbContext>();
        dbContext.GetAllCached<ArticleStageTransition>();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

## Take

**The repository contracts — the whole four-level layering.** This is the largest thing worth taking
and it is entirely absent here: `_116.Shared.Domain.IRepository<T>` is an empty marker, so all 33
repositories (6,195 lines) hand-write the same members. `AlbumRepository.GetByIdOrThrowAsync` and
`ArtistRepository.GetByIdOrThrowAsync` are character-for-character identical apart from the `DbSet`
and the specification type; `GetByIdOrThrowAsync` alone is implemented 13 times, all in Content.

```csharp
IRepository<TEntity, TKey>                     // contract, in Shared
  └── RepositoryBase<TContext, TEntity, TKey>  // EF implementation, every member virtual
        └── Repository<TEntity>                // per-module: closes the TContext generic
              └── AlbumRepository              // per-aggregate: Query() override + its own finders
```

Adopt levels 1–3 verbatim in shape, with three deviations this backend requires — no
`SaveChangesAsync` on the contract (the unit of work owns commit), no public `IQueryable` (it leaks
EF into handlers and defeats the specification pattern), and `*OrThrowAsync` throwing a domain
`NotFoundException` through the existing `FirstDefaultOrThrowAsync` rather than
`InvalidOperationException` from `Single()`. Specified in
[06-repository-contracts.md](06-repository-contracts.md).

**The `Query()` override as the single home for the `Include` graph.** Level 4 of the same layering,
and the reason it is worth the churn. `ArticleRepository` currently spreads 24 `Include` calls across
49 members — `Category` seven times, `PromotionLevel` four, `Images` four, `Tags` three — so whether
a loaded article has its tags depends on which finder was called. That is the class of bug behind
`AddItemTierFactory` reading `item.Tiers` from a method with no `Include`: it worked only because an
earlier tracked load had populated the identity map. One override makes the hydration graph a
guarantee instead of an accident.

**A marker for "this type is reference data".** It makes the caching policy a property of the domain
type instead of a decision re-made at each call site.

**Whole-table caching with an identity lookup served from memory.** Correct for small, static tables.

**Startup warming via `IHostedService`.**

**An explicit fence between cached instances and the change tracker.**

## Reject

- **Inheritance as the composition mechanism.** `AssetTypeDefinitionRepository` *is* a cached repository;
  there is no uncached variant, so there is no way to test the query without the cache, no way to disable
  caching per environment, and the cache dependency is in the constructor of every derived type.
- **No invalidation at all.** `GetOrCreateByType` passes no `MemoryCacheEntryOptions`, so entries live for
  the process lifetime. `IThreadSafeMemoryCache.Remove` exists and **nothing calls it** — the entire
  `ThreadSafeMemoryCache` class is dead code. Acceptable only because their cached data changes on deploy.
- **Type-only keys.** Cannot express a parameterized read.
- **`GetById` throwing `InvalidOperationException` from `Single()`** rather than a domain not-found.
