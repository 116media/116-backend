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

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.GetOwnSessionById.V1;

/// <summary>
/// Response model for retrieving a single session.
/// </summary>
/// <param name="Session">The session DTO associated with the requested ID.</param>
public record AdminGetOwnSessionByIdResponse(SessionDto Session);

/// <summary>
/// Defines the get own session by ID endpoint for authenticated admin users.
/// Handles retrieval of a specific session's details.
/// </summary>
public class AdminGetOwnSessionByIdEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the get own session by ID route within the API pipeline.
    /// Maps the <c>/api/v1/admin/me/sessions/{id:guid}</c> endpoint to handle session retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{IdentityConstants.Me}/{SessionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{IdentityConstants.Me}::{SessionRouteConstants.Endpoint}");

        group
            .MapGet(
                "{id:guid}",
                async (Guid id, ClaimsPrincipal user, IClaimsProvider authProvider, IDispatcher dispatcher) =>
                {
                    Guid userId = authProvider.GetUserIdFromClaims(user: user);

                    var query = new AdminGetOwnSessionByIdQuery(UserId: userId, SessionId: id);
                    AdminGetOwnSessionByIdResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetOwnSessionByIdResponse(Session: result.Session);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminGetOwnSessionByIdMetaField.AdminGetOwnSessionById.Name)
            .WithSummary(summary: AdminGetOwnSessionByIdMetaField.AdminGetOwnSessionById.Summary)
            .WithDescription(description: AdminGetOwnSessionByIdMetaField.AdminGetOwnSessionById.Description)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.SessionManagement)
            .ProducesValidationProblem()
            .Produces<AdminGetOwnSessionByIdResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
