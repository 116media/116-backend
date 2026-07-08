using System.Security.Claims;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticlePromotionFeed.V1;

/// <summary>
/// Response model for the article homepage promotion feed.
/// </summary>
/// <param name="Spot1">Promoted articles for spot 1.</param>
/// <param name="Spot2">Promoted articles for spot 2.</param>
/// <param name="Spot3">Promoted articles for spot 3, split into two columns.</param>
/// <param name="GossipStrip">Gossip articles shown in the horizontal strip below the grid.</param>
public record PublicGetArticlePromotionFeedResponse(
    ArticlePromotionSpotDto Spot1,
    ArticlePromotionSpotDto Spot2,
    ArticlePromotionSpot3Dto Spot3,
    IReadOnlyList<ArticleSummaryDto> GossipStrip
);

/// <summary>
/// Defines the public article promotion feed endpoint.
/// Returns the homepage grid of promoted articles grouped by spot priority.
/// </summary>
public class PublicGetArticlePromotionFeedEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the article promotion feed route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/articles/promotion/feed</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Articles}");

        group
            .MapGet(
                $"/{EditorialRouteConstants.PromotionFeed}",
                async (ClaimsPrincipal user, IClaimsProvider claimsProvider, IDispatcher dispatcher, int? stripSize) =>
                {
                    Guid? userId = null;

                    if (user.Identity?.IsAuthenticated == true)
                    {
                        userId = claimsProvider.GetUserIdFromClaims(user: user);
                    }

                    var query = new PublicGetArticlePromotionFeedQuery(
                        StripSize: stripSize ?? EditorialFeedConstants.DefaultStripSize,
                        CurrentUserId: userId
                    );
                    PublicGetArticlePromotionFeedResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetArticlePromotionFeedResponse(
                        Spot1: result.Spot1,
                        Spot2: result.Spot2,
                        Spot3: result.Spot3,
                        GossipStrip: result.GossipStrip
                    );

                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetArticlePromotionFeedMetaField.GetArticlePromotionFeed.Name)
            .WithSummary(summary: PublicGetArticlePromotionFeedMetaField.GetArticlePromotionFeed.Summary)
            .WithDescription(description: PublicGetArticlePromotionFeedMetaField.GetArticlePromotionFeed.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetArticlePromotionFeedResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
