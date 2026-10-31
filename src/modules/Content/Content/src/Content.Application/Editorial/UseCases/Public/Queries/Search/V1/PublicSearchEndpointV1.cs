using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.BuildingBlocks.Presentation.Constants.RateLimit;
using _116.BuildingBlocks.Presentation.Extensions;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.Search.V1;

/// <summary>
/// Response model for the public search endpoint.
/// </summary>
/// <param name="Query">The trimmed query that was run.</param>
/// <param name="Scope">The scope the results were narrowed to.</param>
/// <param name="Counts">Hit counts keyed by scope name, plus <c>all</c> for the total.</param>
/// <param name="Results">The paginated hits.</param>
public record PublicSearchResponse(
    string Query,
    EnumSearchScope Scope,
    IReadOnlyDictionary<string, int> Counts,
    PaginatedResult<SearchHitDto> Results
)
{
    /// <summary>
    /// Flattens the counts to one entry per scope name so the wire format carries a key per
    /// searchable type, which a new type extends rather than versions.
    /// </summary>
    /// <param name="counts">The counts the catalog reported.</param>
    /// <returns>The counts keyed by lowercase scope name, including <c>all</c>.</returns>
    public static IReadOnlyDictionary<string, int> ToWire(SearchCountsDto counts)
    {
        var wire = new Dictionary<string, int>(counts.ByScope.Count + 1)
        {
            [EnumSearchScope.All.ToString().ToLowerInvariant()] = counts.All,
        };

        foreach ((EnumSearchScope scope, int count) in counts.ByScope)
        {
            wire[scope.ToString().ToLowerInvariant()] = count;
        }

        return wire;
    }
}

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
                    HttpContext httpContext,
                    IDispatcher dispatcher,
                    CancellationToken cancellationToken,
                    EnumSearchScope scope = EnumSearchScope.All,
                    int pageIndex = 0,
                    int pageSize = 20
                ) =>
                {
                    var query = new PublicSearchQuery(
                        PaginatedRequest: new PaginatedRequest(pageIndex, pageSize),
                        Query: q,
                        Scope: scope
                    );

                    PublicSearchResult result = await dispatcher.Send(
                        request: query,
                        cancellationToken: cancellationToken
                    );

                    var response = new PublicSearchResponse(
                        Query: result.Query,
                        Scope: result.Scope,
                        Counts: PublicSearchResponse.ToWire(result.Counts),
                        Results: result.Results
                    );

                    // A query can carry a person's name, so it must not sit in a shared or browser cache.
                    httpContext.Response.Headers.CacheControl = "no-store";

                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicSearchMetaField.Search.Name)
            .WithSummary(summary: PublicSearchMetaField.Search.Summary)
            .WithDescription(description: PublicSearchMetaField.Search.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicSearchResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
