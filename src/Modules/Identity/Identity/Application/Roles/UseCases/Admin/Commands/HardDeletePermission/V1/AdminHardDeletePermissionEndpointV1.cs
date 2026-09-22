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

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeletePermission.V1;

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
                $"{{id}}/{PermissionRouteConstants.Hard}",
                async (string id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
                {
                    var command = new AdminHardDeletePermissionCommand(PermissionId: id);

                    await dispatcher.Send(request: command, cancellationToken: cancellationToken);
                    return Results.NoContent();
                }
            )
            .WithName(endpointName: AdminHardDeletePermissionMetaField.HardDeletePermission.Name)
            .WithSummary(summary: AdminHardDeletePermissionMetaField.HardDeletePermission.Summary)
            .WithDescription(description: AdminHardDeletePermissionMetaField.HardDeletePermission.Description)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentManagement)
            .ProducesValidationProblem()
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
