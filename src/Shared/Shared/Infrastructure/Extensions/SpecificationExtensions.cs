using _116.Shared.Application.Specifications;

using Microsoft.EntityFrameworkCore;

namespace _116.Shared.Infrastructure.Extensions;

/// <summary>
/// Extension methods for applying specifications to Entity Framework queries.
/// These methods bridge the gap between the Specification pattern and EF Core queryable operations.
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Applies a specification to an IQueryable, converting the specification to a Where clause.
    /// This method enables seamless integration between the Specification pattern and EF Core queries.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="query">The queryable to apply the specification to</param>
    /// <param name="specification">The specification containing the filtering logic</param>
    /// <returns>A filtered queryable based on the specification criteria</returns>
    /// <example>
    /// <code>
    /// var activeEntities = context.Entities
    ///     .ApplySpecification(EntitySpecifications.IsActive())
    ///     .ToListAsync();
    /// </code>
    /// </example>
    public static IQueryable<T> ApplySpecification<T>(this IQueryable<T> query, ISpecification<T> specification)
    {
        return query.Where(specification.ToExpression());
    }

    /// <summary>
    /// Finds the first entity matching a specification or throws an exception if not found.
    /// This is a convenience method that combines ApplySpecification with FirstOrDefaultAsync and exception handling.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="query">The queryable to search in</param>
    /// <param name="specification">The specification to match against</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests</param>
    /// <returns>The first matching entity</returns>
    /// <exception cref="InvalidOperationException">Thrown when no entity matches the specification</exception>
    public static async Task<T> FirstBySpecificationAsync<T>(
        this IQueryable<T> query,
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    )
    {
        return await query
            .ApplySpecification(specification)
            .FirstAsync(cancellationToken);
    }

    /// <summary>
    /// Finds the first entity matching a specification or returns null if not found.
    /// This is a convenience method that combines ApplySpecification with FirstOrDefaultAsync.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="query">The queryable to search in</param>
    /// <param name="specification">The specification to match against</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests</param>
    /// <returns>The first matching entity or null if no match is found</returns>
    public static async Task<T?> FirstOrDefaultBySpecificationAsync<T>(
        this IQueryable<T> query,
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    )
    {
        return await query
            .ApplySpecification(specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if any entity matches the given specification.
    /// This is a convenience method that combines ApplySpecification with AnyAsync.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="query">The queryable to check</param>
    /// <param name="specification">The specification to match against</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests</param>
    /// <returns>True if any entity matches the specification, otherwise false</returns>
    public static async Task<bool> AnyBySpecificationAsync<T>(
        this IQueryable<T> query,
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    )
    {
        return await query
            .ApplySpecification(specification)
            .AnyAsync(cancellationToken);
    }
}
