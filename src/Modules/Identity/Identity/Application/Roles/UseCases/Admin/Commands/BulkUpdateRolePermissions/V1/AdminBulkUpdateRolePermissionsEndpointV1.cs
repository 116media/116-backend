using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Roles.Constants;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.BulkUpdateRolePermissions.V1;

/// <summary>
/// Request model for bulk updating role permissions.
/// </summary>
/// <param name="PermissionIds">The list of permission IDs to assign to the role.</param>
public record AdminBulkUpdateRolePermissionsRequest(List<Guid> PermissionIds);

/// <summary>
/// Response model for successful bulk update.
/// </summary>
/// <param name="Role">The role with updated permissions.</param>
public record AdminBulkUpdateRolePermissionsResponse(RoleWithPermissionsDto Role);

/// <summary>
/// Defines the admin bulk update role permissions endpoint.
/// Handles bulk updating permissions for roles.
/// </summary>
public class AdminBulkUpdateRolePermissionsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin bulk update role permissions route within the API pipeline.
    /// Maps the <c>/api/v1/admin/roles/{id}/permissions</c> endpoint (PUT).
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{RoleRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{RoleRouteConstants.Endpoint}");

        group
            .MapPut(
                $"{{id}}/{RoleRouteConstants.Permissions}",
                async (string id, AdminBulkUpdateRolePermissionsRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminBulkUpdateRolePermissionsCommand(
                        RoleId: id,
                        PermissionIds: request.PermissionIds
                    );

                    AdminBulkUpdateRolePermissionsResult result = await dispatcher.Send(request: command);

                    var response = new AdminBulkUpdateRolePermissionsResponse(Role: result.Role);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminBulkUpdateRolePermissionsMetaField.AdminBulkUpdateRolePermissions.Name)
            .WithSummary(summary: AdminBulkUpdateRolePermissionsMetaField.AdminBulkUpdateRolePermissions.Summary)
            .WithDescription(
                description: AdminBulkUpdateRolePermissionsMetaField.AdminBulkUpdateRolePermissions.Description
            )
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminBulkUpdateRolePermissionsResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
