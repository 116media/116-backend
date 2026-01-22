using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Roles.Constants;
using _116.Identity.Application.Shared.Authorizations.Policies;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdateRole.V1;

/// <summary>
/// Request model for updating a role.
/// </summary>
/// <param name="Name">The new name for the role (optional).</param>
/// <param name="Description">The new description for the role (optional).</param>
public record AdminUpdateRoleRequest(string? Name, string? Description);

/// <summary>
/// Response model for successful role update.
/// </summary>
/// <param name="Role">The updated role information.</param>
public record AdminUpdateRoleResponse(RoleDto Role);

/// <summary>
/// Defines the admin update role endpoint.
/// Handles updating existing roles.
/// </summary>
public class AdminUpdateRoleEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin update role route within the API pipeline.
    /// Maps the <c>/api/v1/admin/roles/{id}</c> endpoint to handle role update requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{RoleRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{RoleRouteConstants.Endpoint}");

        group
            .MapPut(
                "{id:guid}",
                async (Guid id, AdminUpdateRoleRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminUpdateRoleCommand(
                        RoleId: id,
                        Name: request.Name,
                        Description: request.Description
                    );

                    AdminUpdateRoleResult result = await dispatcher.Send(request: command);

                    var response = new AdminUpdateRoleResponse(Role: result.Role);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminUpdateRoleMetaField.AdminUpdateRole.Name)
            .WithSummary(summary: AdminUpdateRoleMetaField.AdminUpdateRole.Summary)
            .WithDescription(description: AdminUpdateRoleMetaField.AdminUpdateRole.Description)
            .RequireAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminUpdateRoleResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
