namespace _116.BuildingBlocks.Application.Pagination;

/// <summary>
/// A pagination request whose bounds are enforced at construction. <see cref="PageIndex"/> is clamped to
/// [0, <see cref="MaxPageIndex"/>] and <see cref="PageSize"/> to [1, <see cref="MaxPageSize"/>], so no
/// caller (or query string) can request an unbounded page or an offset that overflows. The previous
/// DataAnnotations <c>[Range]</c> attributes were dead code — minimal APIs never model-bound this
/// record; it is always hand-constructed.
/// </summary>
public record PaginatedRequest
{
    /// <summary>
    /// The maximum number of items a single page may return.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// The highest page index a single request may address. Bounded so that
    /// <see cref="Skip"/> cannot overflow and no caller can ask the store to walk past it.
    /// </summary>
    public const int MaxPageIndex = 10_000;

    /// <summary>
    /// The zero-based index of the page to retrieve (clamped to [0, <see cref="MaxPageIndex"/>]).
    /// </summary>
    public int PageIndex { get; }

    /// <summary>
    /// The number of items per page (clamped to [1, <see cref="MaxPageSize"/>]).
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// The number of rows preceding this page. Both factors are clamped, so the product is
    /// always well below <see cref="int.MaxValue"/> and never wraps negative.
    /// </summary>
    public int Skip => PageIndex * PageSize;

    /// <summary>
    /// Creates a bounded pagination request.
    /// </summary>
    /// <param name="pageIndex">Requested page index; clamped to [0, <see cref="MaxPageIndex"/>].</param>
    /// <param name="pageSize">Requested page size; clamped to [1, <see cref="MaxPageSize"/>].</param>
    /// <example>
    /// <code>
    /// var request = new PaginatedRequest(pageIndex: 2, pageSize: 20);
    /// </code>
    /// </example>
    public PaginatedRequest(int pageIndex = 0, int pageSize = 10)
    {
        PageIndex = Math.Clamp(pageIndex, 0, MaxPageIndex);
        PageSize = Math.Clamp(pageSize, 1, MaxPageSize);
    }
}
