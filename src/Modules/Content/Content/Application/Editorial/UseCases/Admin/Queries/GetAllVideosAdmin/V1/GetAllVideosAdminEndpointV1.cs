using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllVideosAdmin.V1;

/// <summary>
/// Response model for listing all videos.
/// </summary>
/// <param name="Videos">Paginated result containing video summary DTOs and pagination metadata.</param>
public record GetAllVideosAdminResponse(PaginatedResult<VideoSummaryDto> Videos);

/// <summary>
/// Defines the admin get all videos endpoint.
/// Returns a paginated list of videos with optional filters.
/// </summary>
public class GetAllVideosAdminEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the video retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/admin/videos</c> endpoint to handle video listing requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Videos}");

        group
            .MapGet(
                "/",
                async (
                    IDispatcher dispatcher,
                    int pageIndex = 0,
                    int pageSize = 10,
                    EnumContentStatus? status = null,
                    Guid? categoryId = null
                ) =>
                {
                    var paginatedRequest = new PaginatedRequest(pageIndex, pageSize);

                    var query = new GetAllVideosAdminQuery(
                        PaginatedRequest: paginatedRequest,
                        Status: status,
                        CategoryId: categoryId
                    );

                    GetAllVideosAdminResult result = await dispatcher.Send(request: query);

                    var response = new GetAllVideosAdminResponse(Videos: result.Videos);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: GetAllVideosAdminMetaField.GetAllVideosAdmin.Name)
            .WithSummary(summary: GetAllVideosAdminMetaField.GetAllVideosAdmin.Summary)
            .WithDescription(description: GetAllVideosAdminMetaField.GetAllVideosAdmin.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<GetAllVideosAdminResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
