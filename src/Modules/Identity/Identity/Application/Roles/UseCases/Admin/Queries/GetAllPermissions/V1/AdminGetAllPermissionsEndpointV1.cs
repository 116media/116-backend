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

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetAllPermissions.V1;

/// <summary>
/// Response model for retrieving all permissions.
/// </summary>
/// <param name="Permissions">Paginated result containing permission DTOs and pagination metadata.</param>
public record AdminGetAllPermissionsResponse(PaginatedResult<PermissionDto> Permissions);

/// <summary>
/// Defines the admin get all permissions endpoint.
/// Handles retrieval of all permissions with pagination and filtering.
/// </summary>
public class AdminGetAllPermissionsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin get all permissions route within the API pipeline.
    /// Maps the <c>/api/v1/admin/permissions</c> endpoint to handle permission retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{PermissionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{PermissionRouteConstants.Endpoint}");

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

                    var query = new AdminGetAllPermissionsQuery(
                        PaginatedRequest: paginatedRequest,
                        Search: search,
                        IsActive: isActive,
                        IsDeleted: isDeleted
                    );

                    AdminGetAllPermissionsResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetAllPermissionsResponse(Permissions: result.Permissions);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminGetAllPermissionsMetaField.AdminGetAllPermissions.Name)
            .WithSummary(summary: AdminGetAllPermissionsMetaField.AdminGetAllPermissions.Summary)
            .WithDescription(description: AdminGetAllPermissionsMetaField.AdminGetAllPermissions.Description)
            .RequireAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminGetAllPermissionsResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
