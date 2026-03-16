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

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.ActivateRole.V1;

/// <summary>
/// Response model for successful role activation.
/// </summary>
/// <param name="Role">The activated role information.</param>
public record AdminActivateRoleResponse(RoleDto Role);

/// <summary>
/// Defines the admin activate role endpoint.
/// Handles activating roles.
/// </summary>
public class AdminActivateRoleEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin activate role route within the API pipeline.
    /// Maps the <c>/api/v1/admin/roles/{id}/activate</c> endpoint to handle role activation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{RoleRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{RoleRouteConstants.Endpoint}");

        group
            .MapPatch(
                $"{{id}}/{RoleRouteConstants.Activate}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminActivateRoleCommand(RoleId: id);

                    AdminActivateRoleResult result = await dispatcher.Send(request: command);

                    var response = new AdminActivateRoleResponse(Role: result.Role);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminActivateRoleMetaField.AdminActivateRole.Name)
            .WithSummary(summary: AdminActivateRoleMetaField.AdminActivateRole.Summary)
            .WithDescription(description: AdminActivateRoleMetaField.AdminActivateRole.Description)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminActivateRoleResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
