using System.Security.Claims;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Shared.Authorizations.Policies;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Session.UseCases.Public.Queries.GetOwnSessions.V1;

/// <summary>
/// Response model for user sessions.
/// </summary>
/// <param name="Sessions">List of user sessions with metadata.</param>
public record PublicGetOwnSessionsResponse(IReadOnlyCollection<SessionDto> Sessions);

/// <summary>
/// Defines the get own sessions endpoint for authenticated public users.
/// Handles retrieval of all user sessions with optional filtering by status.
/// </summary>
public class PublicGetOwnSessionsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the get own sessions route within the API pipeline.
    /// Maps the <c>/api/v1/public/sessions</c> endpoint to handle session retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{SessionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{SessionRouteConstants.Endpoint}");

        group
            .MapGet(
                "/",
                async (
                    ClaimsPrincipal user,
                    IAuthRepository authRepository,
                    IDispatcher dispatcher,
                    bool? isActive = null
                ) =>
                {
                    Guid userId = authRepository.GetUserIdFromClaims(user: user);

                    var query = new PublicGetOwnSessionsQuery(UserId: userId, IsActive: isActive);
                    PublicGetOwnSessionsResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetOwnSessionsResponse(Sessions: result.Sessions);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: PublicGetOwnSessionsMetaField.PublicGetOwnSessions.Name)
            .WithSummary(summary: PublicGetOwnSessionsMetaField.PublicGetOwnSessions.Summary)
            .WithDescription(description: PublicGetOwnSessionsMetaField.PublicGetOwnSessions.Description)
            .RequireAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.SessionManagement)
            .Produces<PublicGetOwnSessionsResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
