using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos.V1;

/// <summary>
/// Response model for listing published videos.
/// </summary>
/// <param name="Videos">Paginated result containing video summary DTOs and pagination metadata.</param>
public record PublicGetPublishedVideosResponse(PaginatedResult<VideoSummaryDto> Videos);

/// <summary>
/// Defines the public get published videos endpoint.
/// Returns a paginated list of published videos with optional filters.
/// </summary>
public class PublicGetPublishedVideosEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the published videos retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/videos</c> endpoint to handle video listing requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Videos}");

        group
            .MapGet(
                "/",
                async (
                    IDispatcher dispatcher,
                    int pageIndex = 0,
                    int pageSize = 10,
                    string? search = null,
                    Guid? categoryId = null
                ) =>
                {
                    var paginatedRequest = new PaginatedRequest(pageIndex, pageSize);

                    var query = new PublicGetPublishedVideosQuery(
                        PaginatedRequest: paginatedRequest,
                        Search: search,
                        CategoryId: categoryId
                    );

                    PublicGetPublishedVideosResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetPublishedVideosResponse(Videos: result.Videos);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetPublishedVideosMetaField.PublicGetPublishedVideos.Name)
            .WithSummary(summary: PublicGetPublishedVideosMetaField.PublicGetPublishedVideos.Summary)
            .WithDescription(description: PublicGetPublishedVideosMetaField.PublicGetPublishedVideos.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetPublishedVideosResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
