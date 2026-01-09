namespace _116.Shared.Application.Pagination;

/// <summary>
/// A paginated result set containing a subset of data items along with pagination metadata.
/// </summary>
/// <typeparam name="TEntity">The type of items in the result set. Must be a reference type.</typeparam>
/// <example>
/// <code>
/// <![CDATA[
/// var result = new PaginatedResult<ProductDto>(
///     pageIndex: 1,
///     pageSize: 10,
///     count: 100,
///     items: products
/// );
/// ]]>
/// </code>
/// </example>
public class PaginatedResult<TEntity>(int pageIndex, int pageSize, long count, IEnumerable<TEntity> items)
    where TEntity : class
{
    /// <summary>
    /// Gets the current page index (zero-based).
    /// </summary>
    public int PageIndex { get; } = pageIndex;

    /// <summary>
    /// Gets the number of items per page.
    /// </summary>
    public int PageSize { get; } = pageSize;

    /// <summary>
    /// Gets the total number of items across all pages.
    /// </summary>
    public long Count { get; } = count;

    /// <summary>
    /// Gets the items for the current page.
    /// </summary>
    public IEnumerable<TEntity> Items { get; } = items;
}
