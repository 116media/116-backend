using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoPromotionFeed.V1;

/// <summary>
/// Response model for the video homepage promotion feed.
/// </summary>
/// <param name="Spot1">Promoted videos for spot 1.</param>
/// <param name="Spot2">Promoted videos for spot 2.</param>
/// <param name="Spot3">Promoted videos for spot 3, split into two columns.</param>
/// <param name="FreeVideoStrip">Randomly selected free videos shown in the horizontal strip below the grid.</param>
public record PublicGetVideoPromotionFeedResponse(
    VideoPromotionSpotDto Spot1,
    VideoPromotionSpotDto Spot2,
    VideoPromotionSpot3Dto Spot3,
    IReadOnlyList<VideoSummaryDto> FreeVideoStrip
);

/// <summary>
/// Defines the public video promotion feed endpoint.
/// Returns the homepage grid of promoted videos grouped by spot priority.
/// </summary>
public class PublicGetVideoPromotionFeedEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the video promotion feed route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/videos/promotion/feed</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Videos}");

        group
            .MapGet(
                $"/{EditorialRouteConstants.PromotionFeed}",
                async (IDispatcher dispatcher, int? stripSize) =>
                {
                    var query = new PublicGetVideoPromotionFeedQuery(
                        StripSize: stripSize ?? EditorialFeedConstants.DefaultStripSize
                    );
                    PublicGetVideoPromotionFeedResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetVideoPromotionFeedResponse(
                        Spot1: result.Spot1,
                        Spot2: result.Spot2,
                        Spot3: result.Spot3,
                        FreeVideoStrip: result.FreeVideoStrip
                    );

                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetVideoPromotionFeedMetaField.GetVideoPromotionFeed.Name)
            .WithSummary(summary: PublicGetVideoPromotionFeedMetaField.GetVideoPromotionFeed.Summary)
            .WithDescription(description: PublicGetVideoPromotionFeedMetaField.GetVideoPromotionFeed.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetVideoPromotionFeedResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
