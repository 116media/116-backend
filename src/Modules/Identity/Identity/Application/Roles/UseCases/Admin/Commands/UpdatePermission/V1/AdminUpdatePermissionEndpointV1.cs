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

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdatePermission.V1;

/// <summary>
/// Request model for updating a permission.
/// </summary>
/// <param name="Resource">The new resource name (optional).</param>
/// <param name="Action">The new action name (optional).</param>
/// <param name="Description">The new description (optional).</param>
public record AdminUpdatePermissionRequest(string? Resource, string? Action, string? Description);

/// <summary>
/// Response model for successful permission update.
/// </summary>
/// <param name="Permission">The updated permission information.</param>
public record AdminUpdatePermissionResponse(PermissionDto Permission);

/// <summary>
/// Defines the admin update permission endpoint.
/// Handles updating existing permissions.
/// </summary>
public class AdminUpdatePermissionEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin update permission route within the API pipeline.
    /// Maps the <c>/api/v1/admin/permissions/{id}</c> endpoint to handle permission update requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{PermissionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{PermissionRouteConstants.Endpoint}");

        group
            .MapPut(
                "{id:guid}",
                async (Guid id, AdminUpdatePermissionRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminUpdatePermissionCommand(
                        PermissionId: id,
                        Resource: request.Resource,
                        Action: request.Action,
                        Description: request.Description
                    );

                    AdminUpdatePermissionResult result = await dispatcher.Send(request: command);

                    var response = new AdminUpdatePermissionResponse(Permission: result.Permission);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminUpdatePermissionMetaField.AdminUpdatePermission.Name)
            .WithSummary(summary: AdminUpdatePermissionMetaField.AdminUpdatePermission.Summary)
            .WithDescription(description: AdminUpdatePermissionMetaField.AdminUpdatePermission.Description)
            .RequireAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminUpdatePermissionResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
