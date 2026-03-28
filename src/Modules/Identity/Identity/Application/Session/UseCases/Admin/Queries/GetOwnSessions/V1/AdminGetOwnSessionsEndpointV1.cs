using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.GetOwnSessions.V1;

/// <summary>
/// Response model for user sessions.
/// </summary>
/// <param name="Sessions">List of user sessions with metadata.</param>
public record AdminGetOwnSessionsResponse(IReadOnlyCollection<SessionDto> Sessions);

/// <summary>
/// Defines the get own sessions endpoint for authenticated admin users.
/// Handles retrieval of all user sessions with optional filtering by status.
/// </summary>
public class AdminGetOwnSessionsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the get own sessions route within the API pipeline.
    /// Maps the <c>/api/v1/admin/me/sessions</c> endpoint to handle session retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{IdentityConstants.Me}/{SessionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{IdentityConstants.Me}::{SessionRouteConstants.Endpoint}");

        group
            .MapGet(
                "/",
                async (
                    ClaimsPrincipal user,
                    IClaimsProvider authProvider,
                    IDispatcher dispatcher,
                    bool? isActive = null
                ) =>
                {
                    Guid userId = authProvider.GetUserIdFromClaims(user: user);

                    var query = new AdminGetOwnSessionsQuery(UserId: userId, IsActive: isActive);
                    AdminGetOwnSessionsResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetOwnSessionsResponse(Sessions: result.Sessions);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminGetOwnSessionsMetaField.AdminGetOwnSessions.Name)
            .WithSummary(summary: AdminGetOwnSessionsMetaField.AdminGetOwnSessions.Summary)
            .WithDescription(description: AdminGetOwnSessionsMetaField.AdminGetOwnSessions.Description)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.SessionManagement)
            .Produces<AdminGetOwnSessionsResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
