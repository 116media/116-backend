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

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.RestoreRole.V1;

/// <summary>
/// Response model for successful role restoration.
/// </summary>
/// <param name="Role">The restored role information.</param>
public record AdminRestoreRoleResponse(RoleDto Role);

/// <summary>
/// Defines the admin restore role endpoint.
/// Handles restoring soft-deleted roles.
/// </summary>
public class AdminRestoreRoleEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin restore role route within the API pipeline.
    /// Maps the <c>/api/v1/admin/roles/{id}/restore</c> endpoint to handle role restore requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{RoleRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{RoleRouteConstants.Endpoint}");

        group
            .MapPatch(
                $"{{id:guid}}/{RoleRouteConstants.Restore}",
                async (Guid id, IDispatcher dispatcher) =>
                {
                    var command = new AdminRestoreRoleCommand(RoleId: id);

                    AdminRestoreRoleResult result = await dispatcher.Send(request: command);

                    var response = new AdminRestoreRoleResponse(Role: result.Role);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminRestoreRoleMetaField.AdminRestoreRole.Name)
            .WithSummary(summary: AdminRestoreRoleMetaField.AdminRestoreRole.Summary)
            .WithDescription(description: AdminRestoreRoleMetaField.AdminRestoreRole.Description)
            .RequireAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminRestoreRoleResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
