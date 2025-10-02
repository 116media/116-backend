using System.Linq.Expressions;
using _116.Shared.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace _116.Shared.Infrastructure.Extensions;

/// <summary>
/// Provides extension methods for <see cref="DbSet{TEntity}"/>.
/// </summary>
public static class DbSetExtension
{
    /// <summary>
    /// Attempts to find an entity with the specified primary key values.
    /// Throws a <see cref="NotFoundException"/> if the entity is not found.
    /// </summary>
    /// <typeparam name="T">The type of the entity to find.</typeparam>
    /// <param name="dbSet">The <see cref="DbSet{T}"/> to query.</param>
    /// <param name="keyValues">An array of primary key values for the entity.</param>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken"/> to observe while waiting for the task to complete.
    /// </param>
    /// <returns>The found entity of type <typeparamref name="T"/>.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown when an entity with the specified primary key values is not found.
    /// </exception>
    public static async Task<T> FindOrThrowAsync<T>(
        this DbSet<T> dbSet,
        object[] keyValues,
        CancellationToken cancellationToken = default
    ) where T : class
    {
        DbContext context = dbSet.GetService<ICurrentDbContext>().Context;

        T? entity = await context.Set<T>().FindAsync(keyValues, cancellationToken);

        if (entity is null)
        {
            throw new NotFoundException(typeof(T).Name, (object)string.Join(", ", keyValues));
        }

        return entity;
    }

    /// <summary>
    /// Attempts to retrieve a single entity matching the specified predicate from the query.
    /// Throws a <see cref="NotFoundException"/> if no entity is found.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="query">The <see cref="IQueryable{T}"/> to query against.</param>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="asNoTracking">If true, the query will be executed with no tracking.</param>
    /// <param name="keyName">
    /// An optional name for the key used in the predicate (e.g. "sku", "email") for clearer error messages.
    /// </param>
    /// <param name="keyValue">
    /// The actual key value used in the predicate (e.g. "ABC123") for clearer error messages.
    /// </param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The entity that matches the predicate.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no entity matching the predicate is found.
    /// </exception>
    /// <example>
    /// <code>
    /// Var product = await dbContext.Products
    ///     .SingleDefaultOrThrowAsync(p => p.Sku == sku, keyName: "sku", keyValue: sku);
    /// </code>
    /// </example>
    public static async Task<T> SingleDefaultOrThrowAsync<T>(
        this IQueryable<T> query,
        Expression<Func<T, bool>> predicate,
        bool asNoTracking = false,
        string? keyName = null,
        object? keyValue = null,
        CancellationToken cancellationToken = default
    ) where T : class
    {
        if (asNoTracking) query = query.AsNoTracking();

        T? entity = await query.SingleOrDefaultAsync(predicate, cancellationToken);

        if (entity is not null) return entity;
        string entityName = typeof(T).Name;

        throw (keyName, keyValue) switch
        {
            (not null, not null) => new NotFoundException(entityName, keyName, keyValue),
            (null, not null) => new NotFoundException(entityName, keyValue),
            _ => new NotFoundException($"Could Not find {entityName}.")
        };
    }

    /// <summary>
    /// Asynchronously returns a single entity from the query or throws a <see cref="NotFoundException"/> if not found.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="query">The queryable source.</param>
    /// <param name="asNoTracking">Whether to use <c>AsNoTracking()</c> on the query for read-only scenarios.</param>
    /// <param name="keyName">
    /// Optional name of the key (e.g. "username", "email") used in the query for more descriptive error messages.
    /// </param>
    /// <param name="keyValue">
    /// Optional value of the key used to identify the missing entity.
    /// </param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The single entity from the query.</returns>
    /// <exception cref="NotFoundException">
    /// thrown when the query returns no entity.
    /// </exception>
    /// <example>
    /// <code>
    /// var basket = await dbContext.Baskets
    ///     .Where(x => x.UserName == "jean.vincent")
    ///     .SingleDefaultOrThrowAsync(keyName: "username", keyValue: "jean.vincent");
    /// </code>
    /// </example>
    public static async Task<T> SingleDefaultOrThrowAsync<T>(
        this IQueryable<T> query,
        bool asNoTracking = false,
        string? keyName = null,
        object? keyValue = null,
        CancellationToken cancellationToken = default
    ) where T : class
    {
        if (asNoTracking) query = query.AsNoTracking();

        T? entity = await query.SingleOrDefaultAsync(cancellationToken);

        if (entity is not null) return entity;
        string entityName = typeof(T).Name;

        throw (keyName, keyValue) switch
        {
            (not null, not null) => new NotFoundException(entityName, keyName, keyValue),
            (null, not null) => new NotFoundException(entityName, keyValue),
            _ => new NotFoundException($"Could Not find {entityName}.")
        };
    }

    /// <summary>
    /// Asynchronously returns the first entity from the query or throws a <see cref="NotFoundException"/> if no entity is found.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="query">The queryable source.</param>
    /// <param name="asNoTracking">Whether to use <c>AsNoTracking()</c> on the query for read-only scenarios.</param>
    /// <param name="keyName">
    /// Optional name of the key (e.g., "username", "email") used in the query for more descriptive error messages.
    /// </param>
    /// <param name="keyValue">
    /// Optional value of the key used to identify the missing entity.
    /// </param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The first entity from the query.</returns>
    /// <exception cref="NotFoundException">
    /// thrown when the query returns no entity.
    /// </exception>
    /// <example>
    /// <code>
    /// var user = await dbContext.Users
    ///     .Where(x => x.UserName == "jean.vincent")
    ///     .FirstOrDefaultOrThrowAsync(keyName: "username", keyValue: "jean.vincent");
    /// </code>
    /// </example>
    public static async Task<T> FirstDefaultOrThrowAsync<T>(
        this IQueryable<T> query,
        bool asNoTracking = false,
        string? keyName = null,
        object? keyValue = null,
        CancellationToken cancellationToken = default
    ) where T : class
    {
        if (asNoTracking) query = query.AsNoTracking();

        T? entity = await query.FirstOrDefaultAsync(cancellationToken);

        if (entity is not null) return entity;

        string entityName = typeof(T).Name;

        throw (keyName, keyValue) switch
        {
            (not null, not null) => new NotFoundException(entityName, keyName, keyValue),
            (null, not null) => new NotFoundException(entityName, keyValue),
            _ => new NotFoundException($"Could not find {entityName}.")
        };
    }
}
