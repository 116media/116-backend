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

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllLyrics.V1;

/// <summary>
/// Response model for listing all lyrics pages.
/// </summary>
/// <param name="Lyrics">Paginated result containing lyrics DTOs and pagination metadata.</param>
public record AdminGetAllLyricsResponse(PaginatedResult<LyricsDto> Lyrics);

/// <summary>
/// Defines the admin get all lyrics endpoint.
/// Returns a paginated list of lyrics pages.
/// </summary>
public class AdminGetAllLyricsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/admin/lyrics</c> endpoint to handle lyrics listing requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapGet(
                "/",
                async (IDispatcher dispatcher, int pageIndex = 0, int pageSize = 10, string? search = null) =>
                {
                    var paginatedRequest = new PaginatedRequest(pageIndex, pageSize);

                    var query = new AdminGetAllLyricsQuery(PaginatedRequest: paginatedRequest, Search: search);

                    AdminGetAllLyricsResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetAllLyricsResponse(Lyrics: result.Lyrics);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminGetAllLyricsMetaField.AdminGetAllLyrics.Name)
            .WithSummary(summary: AdminGetAllLyricsMetaField.AdminGetAllLyrics.Summary)
            .WithDescription(description: AdminGetAllLyricsMetaField.AdminGetAllLyrics.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminGetAllLyricsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
