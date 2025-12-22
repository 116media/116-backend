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

namespace _116.Identity.Application.Auth.Admin.UseCases.Commands.SignOut.V1;

/// <summary>
/// Request model for admin sign-out (RFC 7009 compliant).
/// </summary>
/// <param name="RefreshToken">The refresh token to revoke.</param>
public record AdminSignOutRequest(
    string RefreshToken
);

/// <summary>
/// Response model for admin sign-out.
/// </summary>
/// <param name="IsSuccess">Indicates if the sign-out operation was successful.</param>
public record AdminSignOutResponse(
    bool IsSuccess
);

/// <summary>
/// Defines the admin sign-out endpoint for authenticated admin users (V1).
/// </summary>
public class AdminSignOutEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin sign-out route within the API pipeline.
    /// Maps the <c>/api/v1/admin/auth/sign-out</c> endpoint to handle sign-out requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{IdentityConstants.Admin}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{IdentityConstants.SchemaName}");
        group.MapPost(AuthRouteConstants.SignOut, async (
                AdminSignOutRequest request,
                ClaimsPrincipal user,
                IAuthRepository authRepository,
                IDispatcher dispatcher
            ) =>
            {
                // Extract user ID from JWT token claims
                Guid userId = authRepository.GetUserIdFromClaims(user);
                var command = new AdminSignOutCommand(userId, request.RefreshToken);
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
