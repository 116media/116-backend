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

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.RestorePermission.V1;

/// <summary>
/// Response model for successful permission restoration.
/// </summary>
/// <param name="Permission">The restored permission information.</param>
public record AdminRestorePermissionResponse(PermissionDto Permission);

/// <summary>
/// Defines the admin restore permission endpoint.
/// Handles restoring soft-deleted permissions.
/// </summary>
public class AdminRestorePermissionEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin restore permission route within the API pipeline.
    /// Maps the <c>/api/v1/admin/permissions/{id}/restore</c> endpoint to handle permission restore requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{PermissionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{PermissionRouteConstants.Endpoint}");

        group
            .MapPatch(
                $"{{id:guid}}/{PermissionRouteConstants.Restore}",
                async (Guid id, IDispatcher dispatcher) =>
                {
                    var command = new AdminRestorePermissionCommand(PermissionId: id);

                    AdminRestorePermissionResult result = await dispatcher.Send(request: command);

                    var response = new AdminRestorePermissionResponse(Permission: result.Permission);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminRestorePermissionMetaField.AdminRestorePermission.Name)
            .WithSummary(summary: AdminRestorePermissionMetaField.AdminRestorePermission.Summary)
            .WithDescription(description: AdminRestorePermissionMetaField.AdminRestorePermission.Description)
            .RequireAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminRestorePermissionResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
