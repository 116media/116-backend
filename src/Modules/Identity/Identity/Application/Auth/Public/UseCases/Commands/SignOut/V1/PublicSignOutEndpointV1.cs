using System.Security.Claims;

using _116.Identity.Application.Shared.Authorizations.Policies;
using _116.Identity.Application.Shared.Constants;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;

using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.Public.UseCases.Commands.SignOut.V1;

/// <summary>
/// Response model for sign-out.
/// </summary>
/// <param name="IsSuccess">Indicates if the sign-out operation was successful.</param>
public record PublicSignOutResponse(
    bool IsSuccess
);
/// <summary>
/// Defines the sign-out endpoint for authenticated public users.
/// </summary>
public class PublicSignOutEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the sign-out route within the API pipeline.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{IdentityConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{IdentityConstants.SchemaName}");
        group.MapDelete(AuthRouteConstants.SignOut, async (
                ClaimsPrincipal user,
                IUserRepository userRepository,
                IDispatcher dispatcher
            ) =>
            {
                // Extract user ID from JWT token claims
                Guid userId = userRepository.GetUserIdFromClaims(user);
                var command = new PublicSignOutCommand(userId);
                PublicSignOutResult result = await dispatcher.Send(command);
                var response = new PublicSignOutResponse(result.IsSuccess);
                return Results.Ok(response);
            })
            .WithName(PublicSignOutMetaField.SignOut.Name)
            .WithSummary(PublicSignOutMetaField.SignOut.Summary)
            .WithDescription(PublicSignOutMetaField.SignOut.Description)
            .RequireAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireAuthorization(AccountStatusPolicies.RequireLoggedInUser)
            .Produces<PublicSignOutResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
