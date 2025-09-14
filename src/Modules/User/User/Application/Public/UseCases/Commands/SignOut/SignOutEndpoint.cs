using _116.BuildingBlocks.Constants;
using _116.Shared.Contracts.Application.CQRS;
using _116.User.Application.Shared.Errors;
using _116.User.Domain.Enums;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace _116.User.Application.Public.UseCases.Commands.SignOut;

/// <summary>
/// Response model for signout.
/// </summary>
/// <param name="IsSuccess">Indicates if the signout operation was successful.</param>
public record SignOutResponse(
    bool IsSuccess
);

/// <summary>
/// Defines the signout endpoint for authenticated public users.
/// </summary>
public class SignOutEndpoint : ICarterModule
{
    /// <summary>
    /// Configures the signout route within the API pipeline.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup(RouteConstants.V1.Public.Auth)
            .WithTags("Public::authentication");

        group.MapPost("/signout", async (ClaimsPrincipal user, IDispatcher dispatcher) =>
            {
                // Extract user ID from JWT token claims
                string? userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                {
                    throw UserErrors.InvalidUserAuthentication();
                }

                // Verify user has the Visitor role (public users only)
                List<string> userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                if (!userRoles.Contains(nameof(CoreUserRole.Visitor)))
                {
                    throw UserErrors.InsufficientPermissions();
                }

                var command = new SignOutCommand(userId);
                SignOutResult result = await dispatcher.Send(command);

                var response = new SignOutResponse(result.IsSuccess);

                return Results.Ok(response);
            })
            .WithName(SignOutMetaField.SignOut.Name)
            .WithSummary(SignOutMetaField.SignOut.Summary)
            .WithDescription(SignOutMetaField.SignOut.Description)
            .RequireAuthorization()
            .Produces<SignOutResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
