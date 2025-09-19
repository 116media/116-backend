using _116.BuildingBlocks.Constants;
using _116.Shared.Contracts.Application.CQRS;
using _116.User.Application.Shared.Authorizations.Policies;
using _116.User.Application.Shared.Repositories;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace _116.User.Application.Admin.UseCases.Commands.SignOut;

/// <summary>
/// Response model for admin sign-out.
/// </summary>
/// <param name="IsSuccess">Indicates if the sign-out operation was successful.</param>
public record AdminSignOutResponse(
    bool IsSuccess
);

/// <summary>
/// Defines the admin sign-out endpoint for authenticated admin users.
/// </summary>
public class AdminSignOutEndpoint : ICarterModule
{
    /// <summary>
    /// Configures the admin sign-out route within the API pipeline.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup(RouteConstants.V1.Admin.Auth)
            .WithTags("Admin::authentication");

        group.MapDelete("/signout", async (ClaimsPrincipal user, IUserRepository userRepository, IDispatcher dispatcher) =>
            {
                // Extract user ID from JWT token claims
                Guid userId = userRepository.GetUserIdFromClaims(user);

                var command = new AdminSignOutCommand(userId);
                AdminSignOutResult result = await dispatcher.Send(command);

                var response = new AdminSignOutResponse(result.IsSuccess);

                return Results.Ok(response);
            })
            .WithName(AdminSignOutMetaField.AdminSignOut.Name)
            .WithSummary(AdminSignOutMetaField.AdminSignOut.Summary)
            .WithDescription(AdminSignOutMetaField.AdminSignOut.Description)
            .RequireAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .Produces<AdminSignOutResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
