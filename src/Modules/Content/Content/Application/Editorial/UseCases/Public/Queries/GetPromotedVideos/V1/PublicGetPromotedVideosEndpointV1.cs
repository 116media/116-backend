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

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedVideos.V1;

/// <summary>
/// Response model for listing promoted videos.
/// </summary>
/// <param name="Videos">The list of promoted video summary DTOs.</param>
public record PublicGetPromotedVideosResponse(IReadOnlyList<VideoSummaryDto> Videos);

/// <summary>
/// Defines the public get promoted videos endpoint.
/// Returns the list of currently promoted published videos.
/// </summary>
public class PublicGetPromotedVideosEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the promoted videos retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/videos/promoted</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Videos}");

        group
            .MapGet(
                $"/{EditorialRouteConstants.Promoted}",
                async (IDispatcher dispatcher) =>
                {
                    var query = new PublicGetPromotedVideosQuery();
                    PublicGetPromotedVideosResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetPromotedVideosResponse(Videos: result.Videos);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetPromotedVideosMetaField.GetPromotedVideos.Name)
            .WithSummary(summary: PublicGetPromotedVideosMetaField.GetPromotedVideos.Summary)
            .WithDescription(description: PublicGetPromotedVideosMetaField.GetPromotedVideos.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetPromotedVideosResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
