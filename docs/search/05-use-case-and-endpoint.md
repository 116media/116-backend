# Use Case and Endpoint

## Checklist

- [ ] `PublicSearchQuery` with `IQuery<PublicSearchResult>`
- [ ] `PublicSearchValidator` enforcing length and scope
- [ ] `PublicSearchHandler` resolving counts, hits, images and category names
- [ ] `PublicSearchMetaField` with the route metadata
- [ ] `PublicSearchEndpointV1` under `ContentBrowsing`
- [ ] Route registered and visible in Swagger

---

## Query

```csharp
using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.Search;

/// <summary>
/// Query for the public cross-content search. Returns the ranked hits for the active
/// scope together with the hit counts for every scope.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters.</param>
/// <param name="Query">The reader's query; trimmed and validated to 2-120 characters.</param>
/// <param name="Scope">The content types to narrow to.</param>
public record PublicSearchQuery(
    PaginatedRequest PaginatedRequest,
    string Query,
    EnumSearchScope Scope = EnumSearchScope.All
) : IQuery<PublicSearchResult>;
```

**Not** `ICacheableRequest`. The list endpoints already document why
(`PublicGetPublishedArticlesQuery`): free-text search produces an unbounded key space.
See [06-logging-and-caching.md](06-logging-and-caching.md).

---

## Validator

```csharp
using _116.BuildingBlocks.Application.Validation;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.Search;

/// <summary>
/// Validates the public search query: a non-empty term of 2 to 120 characters and a
/// scope within the supported enum.
/// </summary>
public class PublicSearchValidator : AbstractValidator<PublicSearchQuery>
{
    private const int MinLength = 2;
    private const int MaxLength = 120;

    /// <summary>
    /// Configures the rules for <see cref="PublicSearchQuery" />.
    /// </summary>
    public PublicSearchValidator()
    {
        RuleFor(query => query.Query)
            .NotEmpty()
            .MinimumLength(MinLength)
            .MaximumLength(MaxLength);

        RuleFor(query => query.Scope).IsInEnum();

        RuleFor(query => query.PaginatedRequest.PageSize).InclusiveBetween(1, 50);
        RuleFor(query => query.PaginatedRequest.PageIndex).GreaterThanOrEqualTo(0);
    }
}
```

The 2-character floor is the same rule the client enforces before issuing a request; it
is repeated here because the client is not a trust boundary.

---

## Handler

```csharp
using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Helpers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.Search;

/// <summary>
/// Handles the <see cref="PublicSearchQuery" />: runs the scoped search, merges the
/// per-type hits into one ranked page, and resolves their display fields in batches.
/// </summary>
/// <param name="searchRepository">Repository running the ranked full-text queries.</param>
/// <param name="searchHitDtoService">Service resolving posters and category names for a page of hits.</param>
public class PublicSearchHandler(
    ISearchRepository searchRepository,
    ISearchHitDtoService searchHitDtoService
) : IQueryHandler<PublicSearchQuery, PublicSearchResult>
{
    /// <inheritdoc />
    public async Task<PublicSearchResult> Handle(
        PublicSearchQuery query,
        CancellationToken cancellationToken
    )
    {
        string term = query.Query.Trim();

        SearchCountsDto counts = await searchRepository.CountAsync(term, cancellationToken);

        IReadOnlyList<SearchHitDto> hits = await ResolveHitsAsync(
            term: term,
            scope: query.Scope,
            request: query.PaginatedRequest,
            ct: cancellationToken
        );

        IReadOnlyList<SearchHitDto> resolved = await searchHitDtoService.ResolveDisplayFieldsAsync(
            hits: hits,
            ct: cancellationToken
        );

        int total = query.Scope switch
        {
            EnumSearchScope.Articles => counts.Articles,
            EnumSearchScope.Videos => counts.Videos,
            _ => counts.All,
        };

        return new PublicSearchResult(
            Query: term,
            Scope: query.Scope,
            Counts: counts,
            Results: new PaginatedResult<SearchHitDto>(
                Items: resolved,
                PageIndex: query.PaginatedRequest.PageIndex,
                PageSize: query.PaginatedRequest.PageSize,
                Count: total
            )
        );
    }

    /// <summary>
    /// Runs the query against the scoped repositories. The `All` scope over-fetches each
    /// type by a page so the merge has enough candidates to order correctly.
    /// </summary>
    /// <param name="term">The trimmed query.</param>
    /// <param name="scope">The active scope.</param>
    /// <param name="request">Pagination parameters.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The page of hits, highest score first.</returns>
    private async Task<IReadOnlyList<SearchHitDto>> ResolveHitsAsync(
        string term,
        EnumSearchScope scope,
        PaginatedRequest request,
        CancellationToken ct
    )
    {
        int skip = request.PageIndex * request.PageSize;

        if (scope == EnumSearchScope.Articles)
        {
            return await searchRepository.SearchArticlesAsync(term, skip, request.PageSize, ct);
        }

        if (scope == EnumSearchScope.Videos)
        {
            return await searchRepository.SearchVideosAsync(term, skip, request.PageSize, ct);
        }

        int window = skip + request.PageSize;

        IReadOnlyList<SearchHitDto> articles = await searchRepository.SearchArticlesAsync(
            term,
            0,
            window,
            ct
        );
        IReadOnlyList<SearchHitDto> videos = await searchRepository.SearchVideosAsync(
            term,
            0,
            window,
            ct
        );

        return
        [
            .. articles
                .Concat(videos)
                .OrderByDescending(hit => hit.Score)
                .ThenByDescending(hit => hit.PublishedAt)
                .Skip(skip)
                .Take(request.PageSize),
        ];
    }
}
```

### The over-fetch

Merging two independently-ordered lists cannot be paginated by asking each side for
`skip..skip+take`: the global page 2 may be entirely articles. The handler therefore
asks each side for `0..skip+take` and slices after merging.

Cost is bounded by the `pageSize` cap of 50 and the fact that nobody pages deep into
search results. Documented so it is not mistaken for an oversight.

---

## Display-field service

Keeps the handler at four dependencies and keeps the batching in one place.

```csharp
/// <summary>
/// Fills the display fields a search hit cannot carry out of the database: the poster
/// URL from Storage and the category name from the catalogue, each in one batch.
/// </summary>
public interface ISearchHitDtoService
{
    /// <summary>
    /// Returns the hits with their poster URLs and category names resolved.
    /// </summary>
    /// <param name="hits">The hits to complete.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The completed hits, in the order given.</returns>
    Task<IReadOnlyList<SearchHitDto>> ResolveDisplayFieldsAsync(
        IReadOnlyList<SearchHitDto> hits,
        CancellationToken ct = default
    );
}
```

Implementation resolves `IFileStorageService.ResolveManyAsync` once for every poster id
and `ICategoryRepository.GetByIdsAsync` once for every category id, then rewrites each
hit with `with { ImageUrl = …, CategoryName = …, Excerpt = ExcerptHelper.Truncate(…) }`.

---

## MetaField

```csharp
namespace _116.Content.Application.Editorial.UseCases.Public.Queries.Search;

/// <summary>
/// Route metadata for the public search endpoint.
/// </summary>
public static class PublicSearchMetaField
{
    /// <summary>
    /// The public search route.
    /// </summary>
    public static readonly RouteMetadata Search = new(
        "PublicSearch",
        "Search across the published content",
        """
            Searches the published articles and videos with PostgreSQL full-text search,
            ranked by relevance. Accent-insensitive and stemmed for French. Returns the
            hits for the requested scope plus the hit counts for every scope, so a client
            can label its scope tabs from one response.
            \n
            **Response Codes:**\n
            - Returns 200 OK with the ranked hits, or an empty list when nothing matches\n
            - Returns 400 Bad Request when the term is shorter than 2 characters\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
```

---

## Endpoint

```csharp
using _116.BuildingBlocks.Presentation.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using Carter;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.Search.V1;

/// <summary>
/// Response model for the public search endpoint.
/// </summary>
/// <param name="Query">The trimmed query that was run.</param>
/// <param name="Scope">The scope the results were narrowed to.</param>
/// <param name="Counts">Hit counts per scope.</param>
/// <param name="Results">The paginated hits.</param>
public record PublicSearchResponse(
    string Query,
    EnumSearchScope Scope,
    SearchCountsDto Counts,
    PaginatedResult<SearchHitDto> Results
);

/// <summary>
/// Defines the public cross-content search endpoint.
/// Returns ranked hits for a query, narrowed to an optional scope.
/// </summary>
internal class PublicSearchEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the public search route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/search</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Search}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Search}");

        group
            .MapGet(
                "/",
                async (
                    string q,
                    IDispatcher dispatcher,
                    CancellationToken cancellationToken,
                    EnumSearchScope scope = EnumSearchScope.All,
                    int pageIndex = 0,
                    int pageSize = 20
                ) =>
                {
                    PublicSearchResult result = await dispatcher.Send(
                        request: new PublicSearchQuery(
                            PaginatedRequest: new PaginatedRequest(pageIndex, pageSize),
                            Query: q,
                            Scope: scope
                        ),
                        cancellationToken: cancellationToken
                    );

                    var response = new PublicSearchResponse(
                        Query: result.Query,
                        Scope: result.Scope,
                        Counts: result.Counts,
                        Results: result.Results
                    );

                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicSearchMetaField.Search.Name)
            .WithApiVersionSet(VersionSets.Default)
            .MapToApiVersion(1)
            .AllowAnonymous()
            .WithRateLimiting(RateLimitPolicies.ContentBrowsing);
    }
}
```

`EditorialRouteConstants.Search` is a new constant, `"search"`.
