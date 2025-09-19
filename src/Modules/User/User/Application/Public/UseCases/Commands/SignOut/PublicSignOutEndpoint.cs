using _116.BuildingBlocks.Constants;
using _116.Shared.Contracts.Application.CQRS;
using _116.User.Application.Shared.Authorizations.Policies;
using _116.User.Application.Shared.Repositories;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace _116.User.Application.Public.UseCases.Commands.SignOut;

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
public class PublicSignOutEndpoint : ICarterModule
{
    /// <summary>
    /// Configures the sign-out route within the API pipeline.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup(RouteConstants.V1.Public.Auth)
            .WithTags("Public::authentication");

        group.MapDelete("/signout", async (ClaimsPrincipal user, IUserRepository userRepository, IDispatcher dispatcher) =>
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
