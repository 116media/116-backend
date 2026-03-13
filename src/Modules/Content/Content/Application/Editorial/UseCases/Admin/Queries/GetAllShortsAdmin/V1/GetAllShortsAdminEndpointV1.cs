using _116.BuildingBlocks.Constants.Authorization.Policies;
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

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllShortsAdmin.V1;

/// <summary>
/// Response model for listing all short videos.
/// </summary>
/// <param name="ShortVideos">Paginated result containing short video DTOs and pagination metadata.</param>
public record GetAllShortsAdminResponse(PaginatedResult<ShortVideoDto> ShortVideos);

/// <summary>
/// Defines the admin get all short videos endpoint.
/// Returns a paginated list of short videos with optional filters.
/// </summary>
public class GetAllShortsAdminEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the short videos retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/admin/shorts</c> endpoint to handle short video listing requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Shorts}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Shorts}");

        group
            .MapGet(
                "/",
                async (IDispatcher dispatcher, int pageIndex = 0, int pageSize = 10, bool? isActive = null) =>
                {
                    var paginatedRequest = new PaginatedRequest(pageIndex, pageSize);

                    var query = new GetAllShortsAdminQuery(PaginatedRequest: paginatedRequest, IsActive: isActive);

                    GetAllShortsAdminResult result = await dispatcher.Send(request: query);

                    var response = new GetAllShortsAdminResponse(ShortVideos: result.ShortVideos);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: GetAllShortsAdminMetaField.GetAllShortsAdmin.Name)
            .WithSummary(summary: GetAllShortsAdminMetaField.GetAllShortsAdmin.Summary)
            .WithDescription(description: GetAllShortsAdminMetaField.GetAllShortsAdmin.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<GetAllShortsAdminResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
