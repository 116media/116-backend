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

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.DeactivateRole.V1;

/// <summary>
/// Response model for successful role deactivation.
/// </summary>
/// <param name="Role">The deactivated role information.</param>
public record AdminDeactivateRoleResponse(RoleDto Role);

/// <summary>
/// Defines the admin deactivate role endpoint.
/// Handles deactivating roles.
/// </summary>
public class AdminDeactivateRoleEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin deactivate role route within the API pipeline.
    /// Maps the <c>/api/v1/admin/roles/{id}/deactivate</c> endpoint to handle role deactivation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{RoleRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{RoleRouteConstants.Endpoint}");

        group
            .MapPatch(
                $"{{id:guid}}/{RoleRouteConstants.Deactivate}",
                async (Guid id, IDispatcher dispatcher) =>
                {
                    var command = new AdminDeactivateRoleCommand(RoleId: id);

                    AdminDeactivateRoleResult result = await dispatcher.Send(request: command);

                    var response = new AdminDeactivateRoleResponse(Role: result.Role);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminDeactivateRoleMetaField.AdminDeactivateRole.Name)
            .WithSummary(summary: AdminDeactivateRoleMetaField.AdminDeactivateRole.Summary)
            .WithDescription(description: AdminDeactivateRoleMetaField.AdminDeactivateRole.Description)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminDeactivateRoleResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
