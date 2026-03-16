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

namespace _116.Identity.Application.Roles.UseCases.Public.Queries.GetOwnRoles.V1;

/// <summary>
/// Response model for the authenticated user's roles and permissions.
/// </summary>
/// <param name="Roles">The list of roles assigned to the user, each with their full permission set.</param>
public record PublicGetOwnRolesResponse(IReadOnlyList<RoleWithPermissionsDto> Roles);

/// <summary>
/// Defines the public get own roles' endpoint.
/// Returns all roles with permissions assigned to the authenticated user.
/// </summary>
public class PublicGetOwnRolesEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the public get own roles route within the API pipeline.
    /// Maps the <c>/api/v1/public/me/roles</c> endpoint to handle role retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{IdentityConstants.Me}")
            .WithTags($"{IdentityConstants.Public}::{IdentityConstants.Me}");

        group
            .MapGet(
                RoleRouteConstants.Endpoint,
                async (ClaimsPrincipal user, IClaimsProvider authProvider, IDispatcher dispatcher) =>
                {
                    Guid userId = authProvider.GetUserIdFromClaims(user: user);

                    var query = new PublicGetOwnRolesQuery(UserId: userId);
                    PublicGetOwnRolesResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetOwnRolesResponse(Roles: result.Roles);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: PublicGetOwnRolesMetaField.GetOwnRoles.Name)
            .WithSummary(summary: PublicGetOwnRolesMetaField.GetOwnRoles.Summary)
            .WithDescription(description: PublicGetOwnRolesMetaField.GetOwnRoles.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.UserProfile)
            .Produces<PublicGetOwnRolesResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
