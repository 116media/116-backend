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

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllArtists.V1;

/// <summary>
/// Response model for listing all artist profiles.
/// </summary>
/// <param name="Artists">Paginated result containing artist profile DTOs and pagination metadata.</param>
public record AdminGetAllArtistsResponse(PaginatedResult<ArtistDto> Artists);

/// <summary>
/// Defines the admin get all artists endpoint.
/// Returns a paginated list of artist profiles.
/// </summary>
public class AdminGetAllArtistsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the artist profile retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/admin/artists</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Artists}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Artists}");

        group
            .MapGet(
                "/",
                async (IDispatcher dispatcher, int pageIndex = 0, int pageSize = 10, string? search = null) =>
                {
                    var paginatedRequest = new PaginatedRequest(pageIndex, pageSize);

                    var query = new AdminGetAllArtistsQuery(PaginatedRequest: paginatedRequest, Search: search);

                    AdminGetAllArtistsResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetAllArtistsResponse(Artists: result.Artists);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminGetAllArtistsMetaField.GetAllArtists.Name)
            .WithSummary(summary: AdminGetAllArtistsMetaField.GetAllArtists.Summary)
            .WithDescription(description: AdminGetAllArtistsMetaField.GetAllArtists.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminGetAllArtistsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
