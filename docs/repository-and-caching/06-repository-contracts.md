# Repository contracts

Adopting the four-level layering from `dotnet-microservices`, with three deliberate deviations
this backend requires.

## The problem being solved

33 repository files, 6,195 lines across all modules, and the same four members hand-written in
almost every one of them. `AlbumRepository` and `ArtistRepository` differ only in the `DbSet` and
the specification type:

```csharp
// AlbumRepository.cs:29-36
public async Task<AlbumEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default)
{
    var specification = new AlbumByIdSpecification(id: id);
    return await context
        .Albums.AsTracking()
        .ApplySpecification(specification: specification)
        .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
}

// ArtistRepository.cs:39-45 — character-for-character identical apart from Album/Artist
```

The second problem is hydration drift. `ArticleRepository` has 24 `Include` calls spread across 49
members — `Category` seven times, `PromotionLevel` four, `Images` four, `Tags` three — so whether a
loaded article has its tags depends on which finder you happened to call. That is the exact class of
bug behind `AddItemTierFactory` reading `item.Tiers` from a method with no `Include`: it worked only
because an earlier tracked load had populated the identity map.

## Level 1 — the contract

`_116.Shared.Domain.IRepository<T>` is an empty marker today. Replace it with a real contract:

```csharp
// src/Shared/Shared/Domain/IRepository.cs
namespace _116.Shared.Domain;

/// <summary>
/// Identity reads and existence probes for an aggregate. Results are untracked; a caller that
/// intends to mutate uses <see cref="IWriteRepository{TEntity, TId}" />.
/// </summary>
/// <typeparam name="TEntity">The aggregate type.</typeparam>
/// <typeparam name="TId">The aggregate's identifier type.</typeparam>
public interface IReadRepository<TEntity, TId>
    where TEntity : class, IEntity<TId>
    where TId : struct
{
    /// <summary>
    /// Reads an aggregate by identity, or null when none exists.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The aggregate, or null.</returns>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports whether an aggregate with the supplied identity exists.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True when the aggregate exists.</returns>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Throws when no aggregate with the supplied identity exists.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <exception cref="NotFoundException">Thrown when no aggregate has the supplied id.</exception>
    Task ExistsOrThrowAsync(TId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Mutation of an aggregate: the tracked load that mutation requires, and the staging calls the
/// unit of work later commits. Commit itself belongs to <see cref="IUnitOfWork" />, never here.
/// </summary>
/// <typeparam name="TEntity">The aggregate type.</typeparam>
/// <typeparam name="TId">The aggregate's identifier type.</typeparam>
public interface IWriteRepository<TEntity, TId>
    where TEntity : class, IEntity<TId>
    where TId : struct
{
    /// <summary>
    /// Reads a tracked aggregate by identity for mutation.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The tracked aggregate.</returns>
    /// <exception cref="NotFoundException">Thrown when no aggregate has the supplied id.</exception>
    Task<TEntity> GetByIdOrThrowAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages a new aggregate for insertion on the next commit.
    /// </summary>
    /// <param name="entity">The aggregate to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages an existing aggregate for update on the next commit.
    /// </summary>
    /// <param name="entity">The aggregate to update.</param>
    void Update(TEntity entity);

    /// <summary>
    /// Stages an aggregate for deletion on the next commit.
    /// </summary>
    /// <param name="entity">The aggregate to remove.</param>
    void Remove(TEntity entity);
}

/// <summary>
/// The union of both roles, for aggregates whose repository serves read and write paths alike.
/// </summary>
/// <typeparam name="TEntity">The aggregate type.</typeparam>
/// <typeparam name="TId">The aggregate's identifier type.</typeparam>
public interface IRepository<TEntity, TId> : IReadRepository<TEntity, TId>, IWriteRepository<TEntity, TId>
    where TEntity : class, IEntity<TId>
    where TId : struct;
```

Three interfaces, not one. A handler that only probes existence — the 18 interaction handlers
converted to `ExistsOrThrowAsync` — depends on `IReadRepository<ArticleEntity, Guid>` and cannot
see `Remove`. A handler that loads and mutates depends on `IWriteRepository`. The union exists for
the aggregate repositories that genuinely serve both, so nobody is forced to inject two
collaborators to do one job.

Three deliberate departures from the reference contract follow.

### Deviation 1 — no `SaveChangesAsync`, no `ClearTracking`

The reference contract carries both:

```csharp
Task<int> SaveChangesAsync(CancellationToken ct = default);
void ClearTracking();
```

Neither belongs here. `IContentUnitOfWork.CommitAsync()` owns the commit precisely so a handler can
mutate three aggregates through three repositories and commit once. Putting `SaveChangesAsync` on
the repository makes that unexpressible and is the flaw that forces `CachedBasketRepository` to
evict on a save an outer transaction could still roll back.

### Deviation 2 — no public `IQueryable`

The reference contract exposes the query root:

```csharp
IQueryable<TEntity> Query();
IQueryable<TEntity> QueryNotTracked();
```

Do not copy this. A public `IQueryable` lets any handler compose EF expressions, which defeats the
specification pattern, moves persistence concerns into the application layer, and makes every
repository impossible to fake in a unit test. **Keep the query root `protected`** — it is a seam for
derived repositories, not part of the contract.

### Deviation 3 — `*OrThrowAsync` throws a domain not-found

The reference `GetById` is `GetAll().Single(...)`, which surfaces `InvalidOperationException`. This
backend already has the correct primitive, and the base reuses it unchanged:

```csharp
.FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken)
// -> NotFoundException(entityName, keyValue) -> NotFoundExceptionHandler -> localized 404
```

## Level 2 — the generic implementation

```csharp
// src/Shared/Shared/Infrastructure/Repositories/RepositoryBase.cs
namespace _116.Shared.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of <see cref="IRepository{TEntity, TId}" />. Derived types
/// override <see cref="Query" /> to declare the aggregate's hydration graph once.
/// </summary>
/// <typeparam name="TContext">The module database context.</typeparam>
/// <typeparam name="TEntity">The aggregate type.</typeparam>
/// <typeparam name="TId">The aggregate's identifier type.</typeparam>
/// <param name="context">The module database context.</param>
public abstract class RepositoryBase<TContext, TEntity, TId>(TContext context) : IRepository<TEntity, TId>
    where TContext : DbContext
    where TEntity : class, IEntity<TId>
    where TId : struct
{
    /// <summary>
    /// The module database context.
    /// </summary>
    protected TContext Context => context;

    /// <summary>
    /// The query root carrying every navigation this aggregate's reads need. Tracking follows the
    /// module default; override to add <c>Include</c> calls rather than repeating them per finder.
    /// </summary>
    protected virtual IQueryable<TEntity> Query() => context.Set<TEntity>();

    /// <summary>
    /// The tracked query root, carrying the same hydration graph as <see cref="Query" />. Every
    /// read whose result will be mutated goes through this.
    /// </summary>
    protected IQueryable<TEntity> QueryTracked() => Query().AsTracking();

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await Query().FirstOrDefaultAsync(entity => entity.Id.Equals(id), cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> GetByIdOrThrowAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await QueryTracked()
            .Where(entity => entity.Id.Equals(id))
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await context.Set<TEntity>().AnyAsync(entity => entity.Id.Equals(id), cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task ExistsOrThrowAsync(TId id, CancellationToken cancellationToken = default)
    {
        bool exists = await ExistsAsync(id: id, cancellationToken: cancellationToken);

        if (!exists)
        {
            throw new NotFoundException(typeof(TEntity).Name, id);
        }
    }

    /// <inheritdoc />
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await context.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public virtual void Update(TEntity entity)
    {
        context.Set<TEntity>().Update(entity);
    }

    /// <inheritdoc />
    public virtual void Remove(TEntity entity)
    {
        context.Set<TEntity>().Remove(entity);
    }
}
```

**One thing to verify when 0.4 lands:** `entity.Id.Equals(id)` in an expression tree relies on EF
translating `Equals` to SQL equality. It does for `Guid`, and the reference base uses the same form —
but confirm it on the first derived repository rather than assuming it across 33. Adding
`where TId : struct, IEquatable<TId>` documents the intent and picks the typed overload.

**Note the inverted tracking seam.** The reference exposes `Query()` (tracked) and
`QueryNotTracked()`. Here it is `Query()` (module default) and `QueryTracked()`, because
`ContentModule` sets `UseNoTrackingByDefault = true` — the Content `DbSet` is already untracked, and
Stage 9 made `.AsTracking()` the explicit opt-in on write paths. Identity and Core do not opt in, so
the base must not assume either default; `QueryTracked()` is correct in both modules.

`ExistsOrThrowAsync` on the base is what the recent guard conversion hand-wrote four times over.

## Level 3 — close the context generic per module

```csharp
// src/Modules/Content/Content/Infrastructure/Repositories/ContentRepository.cs
namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Base repository for Content aggregates keyed by <see cref="Guid" />.
/// </summary>
/// <param name="context">The Content module database context.</param>
public abstract class ContentRepository<TEntity>(ContentDbContext context)
    : RepositoryBase<ContentDbContext, TEntity, Guid>(context)
    where TEntity : class, IEntity<Guid>;
```

One file per module. Every entity repository below it names one type parameter instead of three.

## Level 4 — the entity repository

`AlbumRepository` today is 107 lines. After:

```csharp
// src/Modules/Content/Content/Infrastructure/Repositories/AlbumRepository.cs
/// <summary>
/// Implementation of <see cref="IAlbumRepository" /> for managing album entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class AlbumRepository(ContentDbContext context) : ContentRepository<AlbumEntity>(context), IAlbumRepository
{
    /// <inheritdoc />
    public async Task<(List<AlbumEntity> Albums, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<AlbumEntity> query = Query();

        if (!string.IsNullOrWhiteSpace(search))
        {
            Specification<AlbumEntity> specification = new AlbumSearchSpecification(search: search);
            query = query.ApplySpecification(specification: specification);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<AlbumEntity> albums = await query
            .OrderBy(a => a.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (albums, totalCount);
    }

    /// <inheritdoc />
    public async Task<(List<AlbumEntity> Albums, int TotalCount)> GetByArtistAsync(
        Guid artistId,
        EnumReleaseType releaseType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        Specification<AlbumEntity> specification = new AlbumByArtistSpecification(artistId: artistId).And(
            new AlbumByReleaseTypeSpecification(releaseType: releaseType)
        );

        IQueryable<AlbumEntity> query = Query().ApplySpecification(specification: specification);

        int totalCount = await query.CountAsync(cancellationToken: cancellationToken);

        // Unknown years sort last, not first — Postgres puts NULL first under DESC, which
        // would head a discography with the records nobody dated. The name tie-break keeps
        // paging stable for two releases from the same year.
        List<AlbumEntity> albums = await query
            .OrderBy(a => a.ReleaseYear == null)
            .ThenByDescending(a => a.ReleaseYear)
            .ThenBy(a => a.Name)
            .Skip(count: (page - 1) * pageSize)
            .Take(count: pageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        return (albums, totalCount);
    }
}
```

`GetByIdAsync`, `GetByIdOrThrowAsync`, `AddAsync` and `Update` are gone — inherited. Only the two
album-specific queries remain, and `IAlbumRepository` narrows to
`IRepository<AlbumEntity, Guid>` plus those two.

## The `Query()` override — the reason to do this at all

For an aggregate whose reads need navigations, the graph is declared once:

```csharp
// src/Modules/Content/Content/Infrastructure/Repositories/ArticleRepository.cs
public class ArticleRepository(ContentDbContext context)
    : ContentRepository<ArticleEntity>(context), IArticleRepository
{
    /// <inheritdoc />
    /// <remarks>
    /// Split into separate round-trips: three collection includes on one query multiply rows
    /// cartesian-wise.
    /// </remarks>
    protected override IQueryable<ArticleEntity> Query()
    {
        return base.Query()
            .Include(article => article.Category)
            .Include(article => article.PromotionLevel)
            .Include(article => article.Images)
            .Include(article => article.Tags)
            .AsSplitQuery();
    }
}
```

Add a navigation once and every finder picks it up. No finder can return a half-loaded article, so
nothing downstream can silently depend on identity-map fixup the way `AddItemTierFactory` did.

**Caveat worth stating in the PR:** a blanket `Query()` override hydrates navigations that some
finders do not need. Where a hot read genuinely wants a leaner graph — the public feeds — that
method calls `Context.Set<ArticleEntity>()` directly and says why in a comment. Two shapes, both
explicit, is the goal; twenty-four scattered `Include` calls is not.

## Impact

| | Before | After |
| --- | --- | --- |
| `IRepository<T>` | empty marker | 3 role interfaces (3 + 4 members, plus their union) |
| Narrowest dependency a guard-only handler can take | the whole repository | `IReadRepository<TEntity, TId>`, 3 members |
| `AlbumRepository` | 107 lines, 6 members | ~60 lines, 2 members |
| `GetByIdOrThrowAsync` implementations | 13, all in Content | 1 |
| `ExistsOrThrowAsync` implementations | 4 hand-written | 1 |
| `Include` declarations for `ArticleEntity` | 24 across 49 members | 1 |

## Relationship to the caching work

None, and that is the point. Caching is a Decorator over `IRequestHandler<,>` backed by
`HybridCache` ([04](04-target-design.md)) — it never touches a repository, so nothing in this
document is a prerequisite for it and nothing here is justified by it.

This work stands on its own evidence: 33 repositories, 6,195 lines, `GetByIdOrThrowAsync` implemented 13
times, all in Content, and 24 `Include` calls scattered across `ArticleRepository`'s 49 members.
Sequence it whenever convenient — the existing integration suite is the whole verification for
B1–B4, since bodies move and behaviour does not.
