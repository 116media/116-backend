using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Roles.Constants;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetAllRoles.V1;

/// <summary>
/// Response model for retrieving all roles.
/// </summary>
/// <param name="Roles">Paginated result containing role DTOs and pagination metadata.</param>
public record AdminGetAllRolesResponse(PaginatedResult<RoleDto> Roles);

/// <summary>
/// Defines the admin get all roles endpoint.
/// Handles retrieval of all roles with pagination and filtering.
/// </summary>
public class AdminGetAllRolesEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin get all roles route within the API pipeline.
    /// Maps the <c>/api/v1/admin/roles</c> endpoint to handle role retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{RoleRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{RoleRouteConstants.Endpoint}");

        group
            .MapGet(
                "/",
                async (
                    IDispatcher dispatcher,
                    int pageIndex = 0,
                    int pageSize = 10,
                    string? search = null,
                    bool? isActive = null,
                    bool? isDeleted = null
                ) =>
                {
                    var paginatedRequest = new PaginatedRequest(pageIndex, pageSize);

                    var query = new AdminGetAllRolesQuery(
                        PaginatedRequest: paginatedRequest,
                        Search: search,
                        IsActive: isActive,
                        IsDeleted: isDeleted
                    );

                    AdminGetAllRolesResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetAllRolesResponse(Roles: result.Roles);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminGetAllRolesMetaField.AdminGetAllRoles.Name)
            .WithSummary(summary: AdminGetAllRolesMetaField.AdminGetAllRoles.Summary)
            .WithDescription(description: AdminGetAllRolesMetaField.AdminGetAllRoles.Description)
            .RequireAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminGetAllRolesResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
