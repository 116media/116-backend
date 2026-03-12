using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Roles.Constants;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetOwnRoles.V1;

/// <summary>
/// Response model for the authenticated admin's roles and permissions.
/// </summary>
/// <param name="Roles">The list of roles assigned to the admin, each with their full permission set.</param>
public record AdminGetOwnRolesResponse(IReadOnlyList<RoleWithPermissionsDto> Roles);

/// <summary>
/// Defines the admin get own roles endpoint.
/// Returns all roles with permissions assigned to the authenticated admin.
/// </summary>
public class AdminGetOwnRolesEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin get own roles route within the API pipeline.
    /// Maps the <c>/api/v1/admin/me/roles</c> endpoint to handle role retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{IdentityConstants.Me}")
            .WithTags($"{IdentityConstants.Admin}::{IdentityConstants.Me}");

        group
            .MapGet(
                RoleRouteConstants.Endpoint,
                async (ClaimsPrincipal user, IClaimsProvider authProvider, IDispatcher dispatcher) =>
                {
                    Guid userId = authProvider.GetUserIdFromClaims(user: user);

                    var query = new AdminGetOwnRolesQuery(UserId: userId);
                    AdminGetOwnRolesResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetOwnRolesResponse(Roles: result.Roles);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminGetOwnRolesMetaField.GetOwnRoles.Name)
            .WithSummary(summary: AdminGetOwnRolesMetaField.GetOwnRoles.Summary)
            .WithDescription(description: AdminGetOwnRolesMetaField.GetOwnRoles.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.UserProfile)
            .Produces<AdminGetOwnRolesResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
