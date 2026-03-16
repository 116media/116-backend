using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.User.UseCases.Admin.Queries.GetUserRoles.V1;

/// <summary>
/// Response model for user roles retrieval.
/// </summary>
/// <param name="Roles">The list of roles assigned to the user.</param>
public record AdminGetUserRolesResponse(IReadOnlyCollection<RoleDto> Roles);

/// <summary>
/// Defines the admin get user roles endpoint.
/// Handles retrieving user's roles.
/// </summary>
public class AdminGetUserRolesEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin get user roles route within the API pipeline.
    /// Maps the <c>/api/v1/admin/users/{id}/roles</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/users")
            .WithTags($"{IdentityConstants.Admin}::users");

        group
            .MapGet(
                "{id}/roles",
                async (string id, IDispatcher dispatcher) =>
                {
                    var query = new AdminGetUserRolesQuery(UserId: id);

                    AdminGetUserRolesResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetUserRolesResponse(Roles: result.Roles);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminGetUserRolesMetaField.AdminGetUserRoles.Name)
            .WithSummary(summary: AdminGetUserRolesMetaField.AdminGetUserRoles.Summary)
            .WithDescription(description: AdminGetUserRolesMetaField.AdminGetUserRoles.Description)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminGetUserRolesResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
