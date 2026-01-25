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

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission.V1;

/// <summary>
/// Response model for successful permission soft deletion.
/// </summary>
/// <param name="Permission">The soft deleted permission information.</param>
public record AdminSoftDeletePermissionResponse(PermissionDto Permission);

/// <summary>
/// Defines the admin soft delete permission endpoint.
/// Handles soft deleting permissions.
/// </summary>
public class AdminSoftDeletePermissionEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin soft delete permission route within the API pipeline.
    /// Maps the <c>/api/v1/admin/permissions/{id}</c> endpoint to handle permission soft delete requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{PermissionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{PermissionRouteConstants.Endpoint}");

        group
            .MapDelete(
                "{id:guid}",
                async (Guid id, IDispatcher dispatcher) =>
                {
                    var command = new AdminSoftDeletePermissionCommand(PermissionId: id);

                    AdminSoftDeletePermissionResult result = await dispatcher.Send(request: command);

                    var response = new AdminSoftDeletePermissionResponse(Permission: result.Permission);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminSoftDeletePermissionMetaField.AdminSoftDeletePermission.Name)
            .WithSummary(summary: AdminSoftDeletePermissionMetaField.AdminSoftDeletePermission.Summary)
            .WithDescription(description: AdminSoftDeletePermissionMetaField.AdminSoftDeletePermission.Description)
            .RequireAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminSoftDeletePermissionResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
