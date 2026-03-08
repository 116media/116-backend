using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Roles.Constants;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeleteRole.V1;

/// <summary>
/// Response model for successful role hard deletion.
/// </summary>
/// <param name="Success">Indicates whether the role was successfully deleted.</param>
public record AdminHardDeleteRoleResponse(bool Success);

/// <summary>
/// Defines the admin hard delete role endpoint.
/// Handles permanently deleting roles.
/// </summary>
public class AdminHardDeleteRoleEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin hard delete role route within the API pipeline.
    /// Maps the <c>/api/v1/admin/roles/{id}/hard</c> endpoint to handle role hard delete requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{RoleRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{RoleRouteConstants.Endpoint}");

        group
            .MapDelete(
                $"{{id:guid}}/{RoleRouteConstants.Hard}",
                async (Guid id, IDispatcher dispatcher) =>
                {
                    var command = new AdminHardDeleteRoleCommand(RoleId: id);

                    AdminHardDeleteRoleResult result = await dispatcher.Send(request: command);

                    var response = new AdminHardDeleteRoleResponse(Success: result.Success);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminHardDeleteRoleMetaField.AdminHardDeleteRole.Name)
            .WithSummary(summary: AdminHardDeleteRoleMetaField.AdminHardDeleteRole.Summary)
            .WithDescription(description: AdminHardDeleteRoleMetaField.AdminHardDeleteRole.Description)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminHardDeleteRoleResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
