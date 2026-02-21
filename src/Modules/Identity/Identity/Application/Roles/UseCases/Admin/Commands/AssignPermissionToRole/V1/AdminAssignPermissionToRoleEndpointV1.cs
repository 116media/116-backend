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

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole.V1;

/// <summary>
/// Request model for assigning a permission to a role.
/// </summary>
/// <param name="PermissionId">The ID of the permission to assign.</param>
public record AdminAssignPermissionToRoleRequest(Guid PermissionId);

/// <summary>
/// Response model for successful permission assignment.
/// </summary>
/// <param name="Role">The role with updated permissions.</param>
public record AdminAssignPermissionToRoleResponse(RoleWithPermissionsDto Role);

/// <summary>
/// Defines the admin assign permission to role endpoint.
/// Handles assigning permissions to roles.
/// </summary>
public class AdminAssignPermissionToRoleEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin assign permission to role route within the API pipeline.
    /// Maps the <c>/api/v1/admin/roles/{id}/permissions</c> endpoint to handle permission assignment requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{RoleRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{RoleRouteConstants.Endpoint}");

        group
            .MapPost(
                $"{{id:guid}}/{RoleRouteConstants.Permissions}",
                async (Guid id, AdminAssignPermissionToRoleRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminAssignPermissionToRoleCommand(
                        RoleId: id,
                        PermissionId: request.PermissionId
                    );

                    AdminAssignPermissionToRoleResult result = await dispatcher.Send(request: command);

                    var response = new AdminAssignPermissionToRoleResponse(Role: result.Role);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminAssignPermissionToRoleMetaField.AdminAssignPermissionToRole.Name)
            .WithSummary(summary: AdminAssignPermissionToRoleMetaField.AdminAssignPermissionToRole.Summary)
            .WithDescription(description: AdminAssignPermissionToRoleMetaField.AdminAssignPermissionToRole.Description)
            .RequireAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminAssignPermissionToRoleResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
