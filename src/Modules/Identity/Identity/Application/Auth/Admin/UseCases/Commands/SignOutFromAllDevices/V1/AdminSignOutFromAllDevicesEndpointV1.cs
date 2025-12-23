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

namespace _116.Identity.Application.Auth.Admin.UseCases.Commands.SignOutFromAllDevices.V1;

/// <summary>
/// Response model for admin sign-out from all devices.
/// </summary>
/// <param name="IsSuccess">Indicates if the sign-out operation was successful.</param>
public record AdminSignOutFromAllDevicesResponse(
    bool IsSuccess
);

/// <summary>
/// Defines the admin sign-out from all devices endpoint for authenticated admin users (V1).
/// </summary>
public class AdminSignOutFromAllDevicesEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin sign-out from all devices route within the API pipeline.
    /// Maps the <c>/api/v1/admin/auth/sign-out-all</c> endpoint to handle sign-out requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{IdentityConstants.Admin}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{IdentityConstants.SchemaName}");

        group.MapPost(AuthRouteConstants.SignOutAll, async (
                ClaimsPrincipal user,
                IAuthRepository authRepository,
                IDispatcher dispatcher
            ) =>
            {
                // Extract user ID from JWT token claims
                Guid userId = authRepository.GetUserIdFromClaims(user);
                var command = new AdminSignOutFromAllDevicesCommand(userId);
                AdminSignOutFromAllDevicesResult result = await dispatcher.Send(command);
                var response = new AdminSignOutFromAllDevicesResponse(result.IsSuccess);
                return Results.Ok(response);
            })
            .WithName(AdminSignOutFromAllDevicesMetaField.AdminSignOutFromAllDevices.Name)
            .WithSummary(AdminSignOutFromAllDevicesMetaField.AdminSignOutFromAllDevices.Summary)
            .WithDescription(AdminSignOutFromAllDevicesMetaField.AdminSignOutFromAllDevices.Description)
            .RequireAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .Produces<AdminSignOutFromAllDevicesResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
