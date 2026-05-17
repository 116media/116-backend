# How to use the Specification Pattern to Clean Up Query Logic in .NET

## The Problem

I was building a content management platform with a .NET 9 backend. The platform manages videos, articles, lyrics, and a bunch of lookup tables. Pretty standard stuff. But the repository layer started getting ugly fast.

Every time I needed to query something, I was writing a new method on the repository. Need to find a video by ID? New method. By slug? Another method. By status? Another one. Search by title? You guessed it.

Here is what the video repository was starting to look like:

```csharp
/// <summary>
/// Retrieves a video by its unique identifier.
/// </summary>
Task<VideoEntity?> GetByIdAsync(Guid id, CancellationToken ct);

/// <summary>
/// Retrieves a video by its URL-safe slug.
/// </summary>
Task<VideoEntity?> GetBySlugAsync(string slug, CancellationToken ct);

/// <summary>
/// Retrieves all videos matching a given content status.
/// </summary>
Task<List<VideoEntity>> GetByStatusAsync(EnumContentStatus status, CancellationToken ct);

/// <summary>
/// Retrieves all videos belonging to a given category.
/// </summary>
Task<List<VideoEntity>> GetByCategoryAsync(Guid categoryId, CancellationToken ct);

/// <summary>
/// Searches videos by title and description.
/// </summary>
Task<List<VideoEntity>> SearchAsync(string search, CancellationToken ct);

/// <summary>
/// Retrieves videos matching both a status and a category.
/// </summary>
Task<List<VideoEntity>> GetByStatusAndCategoryAsync(EnumContentStatus status, Guid categoryId, CancellationToken ct);

/// <summary>
/// Searches videos filtered by a given status.
/// </summary>
Task<List<VideoEntity>> SearchByStatusAsync(string search, EnumContentStatus status, CancellationToken ct);

// ... and it kept growing
```

The real pain was when I needed to combine filters. The admin panel lets you filter videos by status, by category, and search by text, all at the same time. Every combination of optional filters was becoming its own method, or worse, one giant method full of `if` statements:

```csharp
/// <summary>
/// Retrieves a paginated list of videos with optional search, status, and category filters.
/// </summary>
/// <param name="search">Optional search term to filter by title and description.</param>
/// <param name="status">Optional content status filter.</param>
/// <param name="categoryId">Optional category filter.</param>
/// <param name="ct">Token to observe for cancellation requests.</param>
/// <returns>A list of matching video entities.</returns>
public async Task<List<VideoEntity>> GetAllAsync(
    string? search, EnumContentStatus? status, Guid? categoryId, CancellationToken ct)
{
    IQueryable<VideoEntity> query = context.Videos;

    if (!string.IsNullOrWhiteSpace(search))
    {
        string pattern = $"%{search}%";
        query = query.Where(v =>
            EF.Functions.ILike(v.Title, pattern) ||
            (v.Description != null && EF.Functions.ILike(v.Description, pattern)));
    }

    if (status.HasValue)
    {
        query = query.Where(v => v.Status == status.Value);
    }

    if (categoryId.HasValue)
    {
        query = query.Where(v => v.CategoryId == categoryId.Value);
    }

    return await query.ToListAsync(ct);
}
```

This works, but the filtering logic is buried inside the repository. You cannot reuse any of it. If the lyrics module needs the same search pattern, you copy paste the code. If you want to test whether a video matches certain criteria without hitting the database, you are stuck. And every time a new filter comes in, you are back in this method adding another `if` block.

I had the same problem across videos, articles, lyrics, tags, categories, pricing tiers, promotion levels, customers. Every module had its own version of this mess.

## The Idea

I had read about the Specification pattern before but never actually used it in a real project. The idea is simple: take each filtering condition and turn it into its own class. Each class holds one rule, expressed as a `Func<T, bool>` (or more precisely, an `Expression<Func<T, bool>>` so Entity Framework can translate it to SQL). Then you compose these small classes together using `And`, `Or`, and `Not`.

Instead of the repository knowing about every possible filter combination, it just takes a specification and applies it. The caller decides what filters to use.

## The Implementation

First, I defined a simple interface that every specification must implement:

```csharp
/// <summary>
/// Represents a specification pattern that defines filtering criteria for objects of type T.
/// </summary>
/// <typeparam name="T">The type of object the specification applies to.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Converts the specification into a LINQ expression.
    /// </summary>
    /// <returns>An expression that represents the filtering criteria.</returns>
    Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Evaluates whether a given object satisfies the specification.
    /// </summary>
    /// <param name="entity">The object to evaluate.</param>
    /// <returns>True if the object satisfies the specification, otherwise false.</returns>
    bool IsSatisfiedBy(T entity);
}
```

Then I built the base class on top of it:

```csharp
/// <summary>
/// Base class for defining specifications and composing them using logical operators.
/// </summary>
/// <typeparam name="T">The type the specification applies to.</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    /// <summary>
    /// Converts the specification into a LINQ expression tree.
    /// </summary>
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Evaluates whether a given entity satisfies this specification in memory.
    /// </summary>
    /// <param name="entity">The entity to evaluate.</param>
    /// <returns>True if the entity matches the specification.</returns>
    public bool IsSatisfiedBy(T entity)
    {
        Func<T, bool> predicate = ToExpression().Compile();
        return predicate(entity);
    }

    /// <summary>
    /// Combines the current specification with another using logical AND.
    /// </summary>
    public Specification<T> And(Specification<T> other)
        => new AndSpecification<T>(this, other);

    /// <summary>
    /// Combines the current specification with another using logical OR.
    /// </summary>
    public Specification<T> Or(Specification<T> other)
        => new OrSpecification<T>(this, other);

    /// <summary>
    /// Inverts the current specification using logical NOT.
    /// </summary>
    public Specification<T> Not()
        => new NotSpecification<T>(this);
}
```

Two things to note here. The `ToExpression()` method returns an `Expression` tree, not a compiled delegate. This is important because EF Core needs the expression tree to translate it into SQL. If you just use `Func<T, bool>`, EF will pull every row from the database and filter in memory. Bad.

The `IsSatisfiedBy` method compiles the expression and runs it in memory. I use this in unit tests to verify that a specification matches or rejects a given entity without needing a database at all.

The `And`, `Or`, and `Not` methods return composite specifications that combine expression trees at the LINQ level. Here is how they work:

```csharp
/// <summary>
/// Combines two specifications using logical AND.
/// </summary>
public class AndSpecification<T>(Specification<T> left, Specification<T> right) : Specification<T>
{
    /// <summary>
    /// Combines two specification expressions using logical AND.
    /// </summary>
    /// <returns>An expression that evaluates to true when both specifications are satisfied.</returns>
    public override Expression<Func<T, bool>> ToExpression()
    {
        Expression<Func<T, bool>> leftExpr = left.ToExpression();
        Expression<Func<T, bool>> rightExpr = right.ToExpression();

        ParameterExpression param = Expression.Parameter(typeof(T));
        BinaryExpression body = Expression.AndAlso(
            Expression.Invoke(leftExpr, param),
            Expression.Invoke(rightExpr, param)
        );

        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}

/// <summary>
/// Combines two specifications using logical OR.
/// </summary>
public class OrSpecification<T>(Specification<T> left, Specification<T> right) : Specification<T>
{
    /// <summary>
    /// Combines two specification expressions using logical OR.
    /// </summary>
    /// <returns>An expression that evaluates to true when either specification is satisfied.</returns>
    public override Expression<Func<T, bool>> ToExpression()
    {
        Expression<Func<T, bool>> leftExpr = left.ToExpression();
        Expression<Func<T, bool>> rightExpr = right.ToExpression();

        ParameterExpression param = Expression.Parameter(typeof(T));
        BinaryExpression body = Expression.OrElse(
            Expression.Invoke(leftExpr, param),
            Expression.Invoke(rightExpr, param)
        );

        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}

/// <summary>
/// Inverts a specification using the logical NOT.
/// </summary>
public class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> _inner;

    /// <summary>
    /// Initializes a new instance of the NotSpecification class.
    /// </summary>
    /// <param name="inner">The specification to negate.</param>
    public NotSpecification(Specification<T> inner)
    {
        _inner = inner;
    }

    /// <summary>
    /// Inverts the inner specification expression using logical NOT.
    /// </summary>
    /// <returns>An expression that evaluates to true when the inner specification is not satisfied.</returns>
    public override Expression<Func<T, bool>> ToExpression()
    {
        Expression<Func<T, bool>> innerExpr = _inner.ToExpression();
        ParameterExpression param = Expression.Parameter(typeof(T));
        UnaryExpression body = Expression.Not(Expression.Invoke(innerExpr, param));

        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}
```

These three classes are the glue. They take expression trees and stitch them together using `Expression.AndAlso`, `Expression.OrElse`, and `Expression.Not`. EF Core sees the final composed expression and translates the whole thing to SQL. No in-memory evaluation.

With those in place, I created concrete specifications for each filter. For videos:

```csharp
/// <summary>
/// Specification that matches a video by its unique identifier.
/// </summary>
public class VideoByIdSpecification(Guid id) : Specification<VideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<VideoEntity, bool>> ToExpression()
    {
        return video => video.Id == id;
    }
}

/// <summary>
/// Specification that matches videos by their content status.
/// </summary>
public class VideoByStatusSpecification(EnumContentStatus status) : Specification<VideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<VideoEntity, bool>> ToExpression()
    {
        return video => video.Status == status;
    }
}

/// <summary>
/// Specification for full-text search across video Title, Description,
/// MetaTitle, and MetaDescription fields.
/// Uses case-insensitive matching (ILIKE in PostgreSQL).
/// </summary>
public class VideoSearchSpecification(string search) : Specification<VideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<VideoEntity, bool>> ToExpression()
    {
        string pattern = $"%{search}%";
        return video =>
            EF.Functions.ILike(video.Title, pattern)
            || (video.Description != null && EF.Functions.ILike(video.Description, pattern))
            || (video.MetaTitle != null && EF.Functions.ILike(video.MetaTitle, pattern))
            || (video.MetaDescription != null && EF.Functions.ILike(video.MetaDescription, pattern));
    }
}
```

Each one is tiny. Each one does exactly one thing. And they are all testable on their own.

To apply them to EF Core queries, I wrote an extension method:

```csharp
/// <summary>
/// Applies a specification to an IQueryable, converting it to a Where clause.
/// Bridges the Specification pattern with EF Core queries.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <param name="query">The queryable to apply the specification to.</param>
/// <param name="specification">The specification containing the filtering logic.</param>
/// <returns>A filtered queryable based on the specification criteria.</returns>
public static IQueryable<T> ApplySpecification<T>(
    this IQueryable<T> query, ISpecification<T> specification)
{
    return query.Where(specification.ToExpression());
}
```

One line. The specification turns into a `.Where()` clause. EF Core translates the expression tree into a SQL `WHERE` clause. No in-memory filtering.

## Composing Filters with a Builder

The specifications handle individual rules, but I still needed a clean way to combine optional filters. I did not want the repository method to go back to a chain of `if` statements. So I defined a builder interface per module:

```csharp
/// <summary>
/// Interface for building dynamic video queries using specifications.
/// Implements the Builder pattern to construct complex queries without conditional logic.
/// </summary>
public interface IVideoQueryBuilder
{
    /// <summary>
    /// Adds a full-text search filter across Title, Description, MetaTitle, and MetaDescription.
    /// </summary>
    IVideoQueryBuilder WithSearch(string? search);

    /// <summary>
    /// Adds a content status filter to the query.
    /// </summary>
    IVideoQueryBuilder WithStatus(EnumContentStatus? status);

    /// <summary>
    /// Adds a category filter to the query.
    /// </summary>
    IVideoQueryBuilder WithCategory(Guid? categoryId);

    /// <summary>
    /// Builds and returns the final specification.
    /// Returns null if no filters were applied.
    /// </summary>
    Specification<VideoEntity>? Build();
}
```

Then the implementation:

```csharp
/// <summary>
/// Builder for constructing dynamic video queries using specifications.
/// Implements the Builder pattern to eliminate conditional logic in query construction.
/// </summary>
public class VideoQueryBuilder : IVideoQueryBuilder
{
    private Specification<VideoEntity>? _specification;

    /// <summary>
    /// Adds a full-text search filter if the search term is not empty.
    /// </summary>
    /// <param name="search">Optional search term.</param>
    public IVideoQueryBuilder WithSearch(string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) return this;

        var searchSpec = new VideoSearchSpecification(search);
        CombineSpecification(searchSpec);
        return this;
    }

    /// <summary>
    /// Adds a content status filter if a status is provided.
    /// </summary>
    /// <param name="status">Optional content status.</param>
    public IVideoQueryBuilder WithStatus(EnumContentStatus? status)
    {
        if (!status.HasValue) return this;

        var statusSpec = new VideoByStatusSpecification(status.Value);
        CombineSpecification(statusSpec);
        return this;
    }

    /// <summary>
    /// Adds a category filter if a category ID is provided.
    /// </summary>
    /// <param name="categoryId">Optional category identifier.</param>
    public IVideoQueryBuilder WithCategory(Guid? categoryId)
    {
        if (!categoryId.HasValue) return this;

        var categorySpec = new VideoByCategorySpecification(categoryId.Value);
        CombineSpecification(categorySpec);
        return this;
    }

    /// <summary>
    /// Builds the final composed specification, or null if no filters were added.
    /// </summary>
    public Specification<VideoEntity>? Build()
    {
        return _specification;
    }

    /// <summary>
    /// Combines a new specification with the existing one using logical AND.
    /// </summary>
    private void CombineSpecification(Specification<VideoEntity> spec)
    {
        _specification = _specification is null ? spec : _specification.And(spec);
    }
}
```

The builder chains optional filters. If a parameter is null or empty, it skips that filter. If multiple filters are provided, it combines them with `And`. The repository just calls `Build()` and applies the result:

```csharp
/// <summary>
/// Retrieves a paginated list of videos using the specification builder.
/// </summary>
/// <param name="page">The 1-based page number.</param>
/// <param name="pageSize">The number of items per page.</param>
/// <param name="search">Optional search term.</param>
/// <param name="status">Optional content status filter.</param>
/// <param name="categoryId">Optional category filter.</param>
/// <param name="ct">Token to observe for cancellation requests.</param>
/// <returns>A tuple containing the list of videos and the total count.</returns>
public async Task<(List<VideoEntity>, int)> GetAllAsync(
    int page, int pageSize, string? search,
    EnumContentStatus? status, Guid? categoryId,
    CancellationToken ct)
{
    IQueryable<VideoEntity> query = context.Videos;

    Specification<VideoEntity>? spec = new VideoQueryBuilder()
        .WithSearch(search)
        .WithStatus(status)
        .WithCategory(categoryId)
        .Build();

    if (spec is not null)
    {
        query = query.ApplySpecification(spec);
    }

    int totalCount = await query.CountAsync(ct);
    List<VideoEntity> videos = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(ct);

    return (videos, totalCount);
}
```

No `if` chains in the repository. No conditional logic at all. The builder handles all of that.

## Testing

This is where the pattern really paid off. Testing a specification does not require a database. You just create an entity in memory and call `IsSatisfiedBy`:

```csharp
/// <summary>
/// Verifies that a video in Draft status satisfies the Draft specification.
/// </summary>
[Fact]
public void VideoByStatusSpecification_WithMatchingStatus_ShouldReturnTrue()
{
    // Arrange
    VideoEntity video = VideoFactory.Create(categoryId);
    var spec = new VideoByStatusSpecification(EnumContentStatus.Draft);

    // Act
    bool result = spec.IsSatisfiedBy(video);

    // Assert
    result.Should().BeTrue();
}

/// <summary>
/// Verifies that a published video does not satisfy the Draft specification.
/// </summary>
[Fact]
public void VideoByStatusSpecification_WithDifferentStatus_ShouldReturnFalse()
{
    // Arrange
    VideoEntity video = VideoFactory.CreatePublished(categoryId);
    var spec = new VideoByStatusSpecification(EnumContentStatus.Draft);

    // Act
    bool result = spec.IsSatisfiedBy(video);

    // Assert
    result.Should().BeFalse();
}
```

Fast, deterministic, no database setup. I can verify that `ActiveVideoSpecification` correctly excludes archived and rejected videos without spinning up an in-memory database or mocking anything.

For specifications that use `EF.Functions.ILike` (which only works with a real PostgreSQL provider), I at least test that the expression compiles:

```csharp
/// <summary>
/// Verifies that the search specification produces a valid compilable expression.
/// ILike requires PostgreSQL, so we only verify compilation here.
/// </summary>
[Fact]
public void VideoSearchSpecification_ShouldCompileExpression()
{
    // Arrange
    var spec = new VideoSearchSpecification("test");

    // Act
    Func<VideoEntity, bool> predicate = spec.ToExpression().Compile();

    // Assert
    predicate.Should().NotBeNull();
}
```

Not a full integration test, but it catches expression tree errors at compile time rather than at runtime.

## What I Got Out of It

After rolling this out across all modules, a few things changed:

- **Repository methods got shorter.** Instead of growing a new method for every filter combination, each repository has a small set of generic methods that accept specifications. The `GetAllAsync` method on every repository looks almost identical.
- **The filtering logic is reusable.** The `VideoSearchSpecification` works whether you call it from the admin panel, a public API, or a background job. Same class, same SQL, same behavior.
- **New filters are trivial to add.** When I needed an `ActiveVideoSpecification` to exclude archived and rejected videos from a dropdown, I created one small class and was done. No repository changes. No handler changes. Just a new specification.
- **Composition works naturally.** Need active videos in a specific category? `activeSpec.And(categorySpec)`. Need published or promoted videos? `publishedSpec.Or(promotedSpec)`. The `And`, `Or`, and `Not` operators handle it.
- **Unit tests are fast.** I have nearly 100 specification tests across the project and they all run in under a second because none of them hit a database.

The pattern is not free. You end up with more files. A module that had one repository class now has a specifications folder with 5 to 10 small classes, plus a builder. But each file is 10 to 15 lines, does one thing, and is independently testable. I will take that over a 200-line repository method with nested conditionals any day.
