using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllContentTypes;

/// <summary>
/// Query for retrieving all content types.
/// </summary>
/// <param name="Search">
/// Optional search term to filter content types by name (case-insensitive, partial match).
/// </param>
public record AdminGetAllContentTypesQuery(string? Search = null)
    : IQuery<AdminGetAllContentTypesResult>,
        IConditionallyCacheableRequest
{
    /// <inheritdoc />
    /// <remarks>
    /// Free-text search produces an unbounded key space, so those results are never stored.
    /// </remarks>
    public bool IsCacheable => string.IsNullOrWhiteSpace(Search);

    /// <inheritdoc />
    public string CacheKey => "lookup:content_types:admin";

    /// <inheritdoc />
    public TimeSpan Ttl => TimeSpan.FromMinutes(30);

    /// <inheritdoc />
    public IReadOnlyList<string> CacheTags => [ContentCacheTags.Lookups];
}

/// <summary>
/// Result of the <see cref="AdminGetAllContentTypesQuery" /> containing all content types.
/// </summary>
/// <param name="ContentTypes">The list of all content types.</param>
public record AdminGetAllContentTypesResult(IReadOnlyList<ContentTypeDto> ContentTypes);
