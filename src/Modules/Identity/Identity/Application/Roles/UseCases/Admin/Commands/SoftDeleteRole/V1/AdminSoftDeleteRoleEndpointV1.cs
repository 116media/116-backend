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

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole.V1;

/// <summary>
/// Response model for successful role soft deletion.
/// </summary>
/// <param name="Role">The soft deleted role information.</param>
/// <param name="IsSuccess">Indicates whether the role was successfully soft deleted.</param>
public record AdminSoftDeleteRoleResponse(RoleDto Role, bool IsSuccess);

/// <summary>
/// Defines the admin soft delete role endpoint.
/// Handles soft deleting roles.
/// </summary>
public class AdminSoftDeleteRoleEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin soft delete role route within the API pipeline.
    /// Maps the <c>/api/v1/admin/roles/{id}</c> endpoint to handle role soft delete requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{RoleRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{RoleRouteConstants.Endpoint}");

        group
            .MapDelete(
                "{id}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminSoftDeleteRoleCommand(RoleId: id);

                    AdminSoftDeleteRoleResult result = await dispatcher.Send(request: command);

                    var response = new AdminSoftDeleteRoleResponse(Role: result.Role, IsSuccess: result.IsSuccess);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminSoftDeleteRoleMetaField.AdminSoftDeleteRole.Name)
            .WithSummary(summary: AdminSoftDeleteRoleMetaField.AdminSoftDeleteRole.Summary)
            .WithDescription(description: AdminSoftDeleteRoleMetaField.AdminSoftDeleteRole.Description)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminSoftDeleteRoleResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
