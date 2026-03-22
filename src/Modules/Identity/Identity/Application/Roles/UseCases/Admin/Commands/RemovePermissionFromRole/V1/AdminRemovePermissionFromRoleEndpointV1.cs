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

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole.V1;

/// <summary>
/// Response model for successful permission removal.
/// </summary>
/// <param name="Role">The role with updated permissions.</param>
/// <param name="IsSuccess">Indicates whether the permission was successfully removed from the role.</param>
public record AdminRemovePermissionFromRoleResponse(RoleWithPermissionsDto Role, bool IsSuccess);

/// <summary>
/// Defines the admin remove permission from role endpoint.
/// Handles removing permissions from roles.
/// </summary>
public class AdminRemovePermissionFromRoleEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin remove permission from role route within the API pipeline.
    /// Maps the <c>/api/v1/admin/roles/{id}/permissions/{permissionId}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{RoleRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{RoleRouteConstants.Endpoint}");

        group
            .MapDelete(
                $"{{id}}/{RoleRouteConstants.Permissions}/{{permissionId}}",
                async (string id, string permissionId, IDispatcher dispatcher) =>
                {
                    var command = new AdminRemovePermissionFromRoleCommand(RoleId: id, PermissionId: permissionId);

                    AdminRemovePermissionFromRoleResult result = await dispatcher.Send(request: command);

                    var response = new AdminRemovePermissionFromRoleResponse(
                        Role: result.Role,
                        IsSuccess: result.IsSuccess
                    );
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminRemovePermissionFromRoleMetaField.AdminRemovePermissionFromRole.Name)
            .WithSummary(summary: AdminRemovePermissionFromRoleMetaField.AdminRemovePermissionFromRole.Summary)
            .WithDescription(
                description: AdminRemovePermissionFromRoleMetaField.AdminRemovePermissionFromRole.Description
            )
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminRemovePermissionFromRoleResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
