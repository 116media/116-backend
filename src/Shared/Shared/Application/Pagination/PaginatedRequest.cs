namespace _116.Shared.Application.Pagination;

/// <summary>
/// A pagination request whose bounds are enforced at construction. <see cref="PageIndex"/> is floored at
/// 0 and <see cref="PageSize"/> is clamped to [1, <see cref="MaxPageSize"/>], so no caller (or query
/// string) can request an unbounded page. The previous DataAnnotations <c>[Range]</c> attributes were
/// dead code — minimal APIs never model-bound this record; it is always hand-constructed.
/// </summary>
public record PaginatedRequest
{
    /// <summary>
    /// The maximum number of items a single page may return.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// The zero-based index of the page to retrieve (floored at 0).
    /// </summary>
    public int PageIndex { get; }

    /// <summary>
    /// The number of items per page (clamped to [1, <see cref="MaxPageSize"/>]).
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Creates a bounded pagination request.
    /// </summary>
    /// <param name="pageIndex">Requested page index; values below 0 become 0.</param>
    /// <param name="pageSize">Requested page size; clamped to [1, <see cref="MaxPageSize"/>].</param>
    /// <example>
    /// <code>
    /// var request = new PaginatedRequest(pageIndex: 2, pageSize: 20);
    /// </code>
    /// </example>
    public PaginatedRequest(int pageIndex = 0, int pageSize = 10)
    {
        PageIndex = Math.Max(0, pageIndex);
        PageSize = Math.Clamp(pageSize, 1, MaxPageSize);
    }
}
