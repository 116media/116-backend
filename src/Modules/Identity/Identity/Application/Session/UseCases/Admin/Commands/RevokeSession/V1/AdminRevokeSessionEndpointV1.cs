using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Session.Constants;
using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Session.UseCases.Admin.Commands.RevokeSession.V1;

/// <summary>
/// Response model for revoking a session.
/// </summary>
/// <param name="IsSuccess">Indicates whether the session was successfully revoked.</param>
public record AdminRevokeSessionResponse(bool IsSuccess);

/// <summary>
/// Defines the revoke session endpoint for authenticated admin users.
/// Handles revoking (logging out from) a specific session by ID.
/// </summary>
public class AdminRevokeSessionEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the revoke session route within the API pipeline.
    /// Maps the <c>/api/v1/admin/me/sessions/revoke/{id}</c> endpoint to handle session revocation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{IdentityConstants.Me}/{SessionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{IdentityConstants.Me}::{SessionRouteConstants.Endpoint}");

        group
            .MapPost(
                $"{SessionRouteConstants.Revoke}/{{id}}",
                async (string id, ClaimsPrincipal user, IClaimsProvider authProvider, IDispatcher dispatcher) =>
                {
                    Guid userId = authProvider.GetUserIdFromClaims(user: user);

                    var command = new AdminRevokeSessionCommand(UserId: userId, SessionId: id);
                    AdminRevokeSessionResult result = await dispatcher.Send(request: command);

                    var response = new AdminRevokeSessionResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminRevokeSessionMetaField.AdminRevokeSession.Name)
            .WithSummary(summary: AdminRevokeSessionMetaField.AdminRevokeSession.Summary)
            .WithDescription(description: AdminRevokeSessionMetaField.AdminRevokeSession.Description)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.SessionManagement)
            .ProducesValidationProblem()
            .Produces<AdminRevokeSessionResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
