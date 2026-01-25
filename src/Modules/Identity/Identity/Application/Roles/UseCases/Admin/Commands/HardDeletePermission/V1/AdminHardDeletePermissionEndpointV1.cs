using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Roles.Constants;
using _116.Identity.Application.Shared.Authorizations.Policies;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeletePermission.V1;

/// <summary>
/// Response model for successful permission hard deletion.
/// </summary>
/// <param name="Success">Indicates whether the permission was successfully deleted.</param>
public record AdminHardDeletePermissionResponse(bool Success);

/// <summary>
/// Defines the admin hard delete permission endpoint.
/// Handles permanently deleting permissions.
/// </summary>
public class AdminHardDeletePermissionEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin hard delete permission route within the API pipeline.
    /// Maps the <c>/api/v1/admin/permissions/{id}/hard</c> endpoint to handle permission hard delete requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{PermissionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{PermissionRouteConstants.Endpoint}");

        group
            .MapDelete(
                $"{{id:guid}}/{PermissionRouteConstants.Hard}",
                async (Guid id, IDispatcher dispatcher) =>
                {
                    var command = new AdminHardDeletePermissionCommand(PermissionId: id);

                    AdminHardDeletePermissionResult result = await dispatcher.Send(request: command);

                    var response = new AdminHardDeletePermissionResponse(Success: result.Success);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminHardDeletePermissionMetaField.AdminHardDeletePermission.Name)
            .WithSummary(summary: AdminHardDeletePermissionMetaField.AdminHardDeletePermission.Summary)
            .WithDescription(description: AdminHardDeletePermissionMetaField.AdminHardDeletePermission.Description)
            .RequireAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminHardDeletePermissionResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
