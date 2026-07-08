using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed.V1;

/// <summary>
/// Response model for the public video feed.
/// </summary>
/// <param name="Sections">The ordered feed sections, most recently pinned category first.</param>
public record PublicGetVideoFeedResponse(IReadOnlyList<VideoFeedSectionDto> Sections);

/// <summary>
/// Defines the public video feed endpoint.
/// Returns the homepage video feed grouped into one section per pinned category.
/// </summary>
public class PublicGetVideoFeedEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the public video feed route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/videos/feed</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Videos}");

        group
            .MapGet(
                $"/{EditorialRouteConstants.Feed}",
                async (IDispatcher dispatcher) =>
                {
                    PublicGetVideoFeedResult result = await dispatcher.Send(request: new PublicGetVideoFeedQuery());

                    var response = new PublicGetVideoFeedResponse(Sections: result.Sections);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetVideoFeedMetaField.GetVideoFeed.Name)
            .WithSummary(summary: PublicGetVideoFeedMetaField.GetVideoFeed.Summary)
            .WithDescription(description: PublicGetVideoFeedMetaField.GetVideoFeed.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetVideoFeedResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
