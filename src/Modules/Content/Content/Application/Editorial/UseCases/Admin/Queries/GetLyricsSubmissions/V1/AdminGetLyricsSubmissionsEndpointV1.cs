using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetLyricsSubmissions.V1;

/// <summary>
/// Response model for listing the community lyrics submission moderation queue.
/// </summary>
/// <param name="Submissions">Paginated result containing submission DTOs and pagination metadata.</param>
public record AdminGetLyricsSubmissionsResponse(PaginatedResult<LyricsSubmissionDto> Submissions);

/// <summary>
/// Defines the admin get lyrics submissions endpoint.
/// Returns a paginated view of the community lyrics submission moderation queue.
/// </summary>
public class AdminGetLyricsSubmissionsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics submission moderation queue route within the API pipeline.
    /// Maps the <c>GET /api/v1/admin/lyrics/submissions</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapGet(
                $"/{EditorialRouteConstants.Submissions}",
                async (
                    IDispatcher dispatcher,
                    int pageIndex = 0,
                    int pageSize = 10,
                    EnumSubmissionStatus? status = null
                ) =>
                {
                    var paginatedRequest = new PaginatedRequest(pageIndex, pageSize);

                    var query = new AdminGetLyricsSubmissionsQuery(PaginatedRequest: paginatedRequest, Status: status);

                    AdminGetLyricsSubmissionsResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetLyricsSubmissionsResponse(Submissions: result.Submissions);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminGetLyricsSubmissionsMetaField.GetLyricsSubmissions.Name)
            .WithSummary(summary: AdminGetLyricsSubmissionsMetaField.GetLyricsSubmissions.Summary)
            .WithDescription(description: AdminGetLyricsSubmissionsMetaField.GetLyricsSubmissions.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminGetLyricsSubmissionsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
