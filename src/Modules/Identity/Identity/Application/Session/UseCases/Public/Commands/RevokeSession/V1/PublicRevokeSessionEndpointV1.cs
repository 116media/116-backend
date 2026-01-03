using System.Security.Claims;

using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Shared.Authorizations.Policies;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;

using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Session.UseCases.Public.Commands.RevokeSession.V1;

/// <summary>
/// Response model for revoking a session.
/// </summary>
/// <param name="IsSuccess">Indicates whether the session was successfully revoked.</param>
public record PublicRevokeSessionResponse(bool IsSuccess);

/// <summary>
/// Defines the revoke session endpoint for authenticated public users.
/// Handles revoking (logging out from) a specific session by ID.
/// </summary>
public class PublicRevokeSessionEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the revoke session route within the API pipeline.
    /// Maps the <c>/api/v1/public/sessions/revoke/{id: guid}</c> endpoint to handle session revocation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{SessionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{IdentityConstants.SchemaName}");

        group.MapPost($"{SessionRouteConstants.Revoke}/{{id}}", async (
                string id,
                ClaimsPrincipal user,
                IAuthRepository authRepository,
                IDispatcher dispatcher
            ) =>
            {
                Guid userId = authRepository.GetUserIdFromClaims(user: user);

                var command = new PublicRevokeSessionCommand(UserId: userId, SessionId: id);
                PublicRevokeSessionResult result = await dispatcher.Send(request: command);

                var response = new PublicRevokeSessionResponse(IsSuccess: result.IsSuccess);
                return Results.Ok(value: response);
            })
            .WithName(endpointName: PublicRevokeSessionMetaField.PublicRevokeSession.Name)
            .WithSummary(summary: PublicRevokeSessionMetaField.PublicRevokeSession.Summary)
            .WithDescription(description: PublicRevokeSessionMetaField.PublicRevokeSession.Description)
            .RequireAuthorization(UserRolePolicies.RequireVisitorOnly)
            .ProducesValidationProblem()
            .Produces<PublicRevokeSessionResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound);
    }
}
