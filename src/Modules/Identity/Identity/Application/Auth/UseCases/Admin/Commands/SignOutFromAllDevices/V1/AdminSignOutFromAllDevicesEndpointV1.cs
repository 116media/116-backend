using System.Security.Claims;
using _116.BuildingBlocks.Constants.RateLimit;
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

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOutFromAllDevices.V1;

/// <summary>
/// Response model for admin sign-out from all devices.
/// </summary>
/// <param name="IsSuccess">Indicates if the sign-out operation was successful.</param>
public record AdminSignOutFromAllDevicesResponse(bool IsSuccess);

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
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{IdentityConstants.SchemaName}");

        group
            .MapPost(
                pattern: AuthRouteConstants.SignOutAll,
                async (ClaimsPrincipal user, IAuthRepository authRepository, IDispatcher dispatcher) =>
                {
                    Guid userId = authRepository.GetUserIdFromClaims(user: user);

                    var command = new AdminSignOutFromAllDevicesCommand(UserId: userId);
                    AdminSignOutFromAllDevicesResult result = await dispatcher.Send(request: command);

                    var response = new AdminSignOutFromAllDevicesResponse(IsSuccess: result.IsSuccess);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminSignOutFromAllDevicesMetaField.AdminSignOutFromAllDevices.Name)
            .WithSummary(summary: AdminSignOutFromAllDevicesMetaField.AdminSignOutFromAllDevices.Summary)
            .WithDescription(description: AdminSignOutFromAllDevicesMetaField.AdminSignOutFromAllDevices.Description)
            .RequireAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.SessionManagement)
            .Produces<AdminSignOutFromAllDevicesResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
