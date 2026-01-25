using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Shared.Authorizations.Policies;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser.V1;

/// <summary>
/// Response model for successful role removal.
/// </summary>
/// <param name="Roles">The list of remaining roles assigned to the user.</param>
public record AdminRemoveRoleFromUserResponse(IReadOnlyCollection<RoleDto> Roles);

/// <summary>
/// Defines the admin remove role from user endpoint.
/// Handles removing roles from users.
/// </summary>
public class AdminRemoveRoleFromUserEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin remove role from user route within the API pipeline.
    /// Maps the <c>/api/v1/admin/users/{id}/roles/{roleId}</c> endpoint (DELETE).
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/users")
            .WithTags($"{IdentityConstants.Admin}::users");

        group
            .MapDelete(
                "{id:guid}/roles/{roleId:guid}",
                async (Guid id, Guid roleId, IDispatcher dispatcher) =>
                {
                    var command = new AdminRemoveRoleFromUserCommand(UserId: id, RoleId: roleId);

                    AdminRemoveRoleFromUserResult result = await dispatcher.Send(request: command);

                    var response = new AdminRemoveRoleFromUserResponse(Roles: result.Roles);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminRemoveRoleFromUserMetaField.AdminRemoveRoleFromUser.Name)
            .WithSummary(summary: AdminRemoveRoleFromUserMetaField.AdminRemoveRoleFromUser.Summary)
            .WithDescription(description: AdminRemoveRoleFromUserMetaField.AdminRemoveRoleFromUser.Description)
            .RequireAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminRemoveRoleFromUserResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
