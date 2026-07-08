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

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularVideos.V1;

/// <summary>
/// Response model for listing popular videos.
/// </summary>
/// <param name="Videos">The videos ordered by engagement score descending.</param>
public record PublicGetPopularVideosResponse(IReadOnlyList<VideoSummaryDto> Videos);

/// <summary>
/// Defines the public get popular videos endpoint.
/// Returns published videos ranked by a weighted engagement score, cached for 10 minutes.
/// </summary>
public class PublicGetPopularVideosEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the popular videos retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/videos/popular</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Videos}");

        group
            .MapGet(
                $"/{EditorialRouteConstants.Popular}",
                async (
                    IDispatcher dispatcher,
                    int limit = PopularVideosLimits.DefaultLimit,
                    Guid? categoryId = null,
                    Guid? excludeId = null
                ) =>
                {
                    int safeLimit = Math.Clamp(
                        value: limit,
                        min: PopularVideosLimits.MinLimit,
                        max: PopularVideosLimits.MaxLimit
                    );

                    var query = new PublicGetPopularVideosQuery(
                        Limit: safeLimit,
                        CategoryId: categoryId,
                        ExcludeId: excludeId
                    );

                    PublicGetPopularVideosResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetPopularVideosResponse(Videos: result.Videos);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetPopularVideosMetaField.GetPopularVideos.Name)
            .WithSummary(summary: PublicGetPopularVideosMetaField.GetPopularVideos.Summary)
            .WithDescription(description: PublicGetPopularVideosMetaField.GetPopularVideos.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetPopularVideosResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
