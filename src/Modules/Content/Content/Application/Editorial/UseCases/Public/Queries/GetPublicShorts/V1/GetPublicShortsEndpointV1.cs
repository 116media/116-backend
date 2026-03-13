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

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShorts.V1;

/// <summary>
/// Response model for listing active short videos.
/// </summary>
/// <param name="ShortVideos">Paginated result containing short video DTOs and pagination metadata.</param>
public record GetPublicShortsResponse(PaginatedResult<ShortVideoDto> ShortVideos);

/// <summary>
/// Defines the public get short videos endpoint.
/// Returns a paginated list of active short videos available to all users.
/// </summary>
public class GetPublicShortsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the public short videos retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/shorts</c> endpoint to handle short video listing requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Shorts}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Shorts}");

        group
            .MapGet(
                "/",
                async (IDispatcher dispatcher, int pageIndex = 0, int pageSize = 10) =>
                {
                    var paginatedRequest = new PaginatedRequest(pageIndex, pageSize);

                    var query = new GetPublicShortsQuery(PaginatedRequest: paginatedRequest);

                    GetPublicShortsResult result = await dispatcher.Send(request: query);

                    var response = new GetPublicShortsResponse(ShortVideos: result.ShortVideos);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: GetPublicShortsMetaField.GetPublicShorts.Name)
            .WithSummary(summary: GetPublicShortsMetaField.GetPublicShorts.Summary)
            .WithDescription(description: GetPublicShortsMetaField.GetPublicShorts.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<GetPublicShortsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
