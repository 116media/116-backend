using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Lookup.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetPopularTags.V1;

/// <summary>
/// Response model for listing popular tags.
/// </summary>
/// <param name="Tags">The list of popular tags ordered by usage count descending.</param>
public record PublicGetPopularTagsResponse(IReadOnlyList<TagDto> Tags);

/// <summary>
/// Defines the public get popular tags endpoint.
/// Returns the most-used tags across articles and videos, cached for 10 minutes.
/// </summary>
public class PublicGetPopularTagsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the popular tags retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/tags/popular</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{LookupRouteConstants.Tags}/popular")
            .WithTags($"{ContentConstants.Public}::{LookupRouteConstants.Tags}");

        group
            .MapGet(
                "/",
                async (IDispatcher dispatcher, int? limit = null, string? contentType = null) =>
                {
                    EnumCoreContentType? parsedContentType = Enum.TryParse(
                        contentType,
                        ignoreCase: true,
                        out EnumCoreContentType parsed
                    )
                        ? parsed
                        : null;

                    var query = new PublicGetPopularTagsQuery(Limit: limit, ContentType: parsedContentType);

                    PublicGetPopularTagsResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetPopularTagsResponse(Tags: result.Tags);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetPopularTagsMetaField.GetPopularTags.Name)
            .WithSummary(summary: PublicGetPopularTagsMetaField.GetPopularTags.Summary)
            .WithDescription(description: PublicGetPopularTagsMetaField.GetPopularTags.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetPopularTagsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
