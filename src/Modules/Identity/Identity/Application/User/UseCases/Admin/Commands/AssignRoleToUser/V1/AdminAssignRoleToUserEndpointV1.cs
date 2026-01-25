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

namespace _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser.V1;

/// <summary>
/// Request model for assigning a role to a user.
/// </summary>
/// <param name="RoleId">The ID of the role to assign.</param>
public record AdminAssignRoleToUserRequest(Guid RoleId);

/// <summary>
/// Response model for successful role assignment.
/// </summary>
/// <param name="Roles">The list of roles assigned to the user.</param>
public record AdminAssignRoleToUserResponse(IReadOnlyCollection<RoleDto> Roles);

/// <summary>
/// Defines the admin assign role to user endpoint.
/// Handles assigning roles to users.
/// </summary>
public class AdminAssignRoleToUserEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin assign role to user route within the API pipeline.
    /// Maps the <c>/api/v1/admin/users/{id}/roles</c> endpoint (POST).
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/users")
            .WithTags($"{IdentityConstants.Admin}::users");

        group
            .MapPost(
                "{id:guid}/roles",
                async (Guid id, AdminAssignRoleToUserRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminAssignRoleToUserCommand(UserId: id, RoleId: request.RoleId);

                    AdminAssignRoleToUserResult result = await dispatcher.Send(request: command);

                    var response = new AdminAssignRoleToUserResponse(Roles: result.Roles);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminAssignRoleToUserMetaField.AdminAssignRoleToUser.Name)
            .WithSummary(summary: AdminAssignRoleToUserMetaField.AdminAssignRoleToUser.Summary)
            .WithDescription(description: AdminAssignRoleToUserMetaField.AdminAssignRoleToUser.Description)
            .RequireAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminAssignRoleToUserResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
