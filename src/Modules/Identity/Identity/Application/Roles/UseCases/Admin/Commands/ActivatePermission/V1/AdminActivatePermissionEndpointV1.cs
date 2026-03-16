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

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.ActivatePermission.V1;

/// <summary>
/// Response model for successful permission activation.
/// </summary>
/// <param name="Permission">The activated permission information.</param>
public record AdminActivatePermissionResponse(PermissionDto Permission);

/// <summary>
/// Defines the admin activate permission endpoint.
/// Handles activating permissions.
/// </summary>
public class AdminActivatePermissionEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin activate permission route within the API pipeline.
    /// Maps the <c>/api/v1/admin/permissions/{id}/activate</c> endpoint to handle permission activation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{PermissionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{PermissionRouteConstants.Endpoint}");

        group
            .MapPatch(
                $"{{id}}/{PermissionRouteConstants.Activate}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminActivatePermissionCommand(PermissionId: id);

                    AdminActivatePermissionResult result = await dispatcher.Send(request: command);

                    var response = new AdminActivatePermissionResponse(Permission: result.Permission);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminActivatePermissionMetaField.AdminActivatePermission.Name)
            .WithSummary(summary: AdminActivatePermissionMetaField.AdminActivatePermission.Summary)
            .WithDescription(description: AdminActivatePermissionMetaField.AdminActivatePermission.Description)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminActivatePermissionResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
