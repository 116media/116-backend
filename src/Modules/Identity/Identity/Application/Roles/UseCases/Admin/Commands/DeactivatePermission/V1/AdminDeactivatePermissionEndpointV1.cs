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

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.DeactivatePermission.V1;

/// <summary>
/// Response model for successful permission deactivation.
/// </summary>
/// <param name="Permission">The deactivated permission information.</param>
public record AdminDeactivatePermissionResponse(PermissionDto Permission);

/// <summary>
/// Defines the admin deactivate permission endpoint.
/// Handles deactivating permissions.
/// </summary>
public class AdminDeactivatePermissionEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin deactivate permission route within the API pipeline.
    /// Maps the <c>/api/v1/admin/permissions/{id}/deactivate</c> endpoint to handle permission deactivation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{PermissionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{PermissionRouteConstants.Endpoint}");

        group
            .MapPatch(
                $"{{id:guid}}/{PermissionRouteConstants.Deactivate}",
                async (Guid id, IDispatcher dispatcher) =>
                {
                    var command = new AdminDeactivatePermissionCommand(PermissionId: id);

                    AdminDeactivatePermissionResult result = await dispatcher.Send(request: command);

                    var response = new AdminDeactivatePermissionResponse(Permission: result.Permission);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminDeactivatePermissionMetaField.AdminDeactivatePermission.Name)
            .WithSummary(summary: AdminDeactivatePermissionMetaField.AdminDeactivatePermission.Summary)
            .WithDescription(description: AdminDeactivatePermissionMetaField.AdminDeactivatePermission.Description)
            .RequireAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminDeactivatePermissionResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
