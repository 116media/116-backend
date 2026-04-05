using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignOutFromAllDevices.V1;

/// <summary>
/// Response model for sign-out from all devices.
/// </summary>
/// <param name="IsSuccess">Indicates if the sign-out operation was successful.</param>
public record PublicSignOutFromAllDevicesResponse(bool IsSuccess);

/// <summary>
/// Defines the sign-out from all devices endpoint for authenticated public users.
/// </summary>
public class PublicSignOutFromAllDevicesEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the "sign-out from all devices" route within the API pipeline.
    /// Maps the <c>POST /api/v1/public/auth/sign-out-all</c> endpoint to handle sign-out from all device requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{IdentityConstants.SchemaName}");

        group
            .MapPost(
                pattern: AuthRouteConstants.SignOutAll,
                async (
                    ClaimsPrincipal user,
                    IClaimsProvider authProvider,
                    IDispatcher dispatcher,
                    ITokenDeliveryService tokenDelivery
                ) =>
                {
                    Guid userId = authProvider.GetUserIdFromClaims(user: user);

                    var command = new PublicSignOutFromAllDevicesCommand(UserId: userId);
                    PublicSignOutFromAllDevicesResult result = await dispatcher.Send(request: command);

                    if (tokenDelivery.IsWebClient())
                    {
                        tokenDelivery.ClearTokenCookies();
                    }

                    var response = new PublicSignOutFromAllDevicesResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: PublicSignOutFromAllDevicesMetaField.SignOutFromAllDevices.Name)
            .WithSummary(summary: PublicSignOutFromAllDevicesMetaField.SignOutFromAllDevices.Summary)
            .WithDescription(description: PublicSignOutFromAllDevicesMetaField.SignOutFromAllDevices.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.SessionManagement)
            .Produces<PublicSignOutFromAllDevicesResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
