using System.Security.Claims;

using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Shared.Authorizations.Policies;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;

using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOut.V1;

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
            .MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{IdentityConstants.SchemaName}");
        group.MapPost(pattern: AuthRouteConstants.SignOut, async (
                AdminSignOutRequest request,
                ClaimsPrincipal user,
                IAuthRepository authRepository,
                IDispatcher dispatcher
            ) =>
            {
                // Extract user ID from JWT token claims
                Guid userId = authRepository.GetUserIdFromClaims(user: user);
                var command = new AdminSignOutCommand(UserId: userId, RefreshToken: request.RefreshToken);
                AdminSignOutResult result = await dispatcher.Send(request: command);
                var response = new AdminSignOutResponse(IsSuccess: result.IsSuccess);
                return Results.Ok(value: response);
            })
            .WithName(endpointName: AdminSignOutMetaField.AdminSignOut.Name)
            .WithSummary(summary: AdminSignOutMetaField.AdminSignOut.Summary)
            .WithDescription(description: AdminSignOutMetaField.AdminSignOut.Description)
            .RequireAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .Produces<AdminSignOutResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden);
    }
}
