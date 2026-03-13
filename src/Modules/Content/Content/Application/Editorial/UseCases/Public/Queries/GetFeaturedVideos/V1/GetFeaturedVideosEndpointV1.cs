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

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetFeaturedVideos.V1;

/// <summary>
/// Response model for listing featured videos.
/// </summary>
/// <param name="Videos">The list of featured video summary DTOs.</param>
public record GetFeaturedVideosResponse(IReadOnlyList<VideoSummaryDto> Videos);

/// <summary>
/// Defines the public get featured videos endpoint.
/// Returns the list of currently featured published videos.
/// </summary>
public class GetFeaturedVideosEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the featured videos retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/videos/featured</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Videos}");

        group
            .MapGet(
                $"/{EditorialRouteConstants.Featured}",
                async (IDispatcher dispatcher) =>
                {
                    var query = new GetFeaturedVideosQuery();
                    GetFeaturedVideosResult result = await dispatcher.Send(request: query);
                    return Results.Ok(new GetFeaturedVideosResponse(Videos: result.Videos));
                }
            )
            .WithName(endpointName: GetFeaturedVideosMetaField.GetFeaturedVideos.Name)
            .WithSummary(summary: GetFeaturedVideosMetaField.GetFeaturedVideos.Summary)
            .WithDescription(description: GetFeaturedVideosMetaField.GetFeaturedVideos.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<GetFeaturedVideosResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
