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

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut.V1;

/// <summary>
/// Request model for sign-out (RFC 7009 compliant).
/// </summary>
/// <param name="RefreshToken">
/// Optional refresh token to revoke.
/// Web clients send the token via HttpOnly cookies, while mobile clients
/// include it in the request body.
/// </param>
public record PublicSignOutRequest(string? RefreshToken);

/// <summary>
/// Response model for sign-out.
/// </summary>
/// <param name="IsSuccess">Indicates if the sign-out operation was successful.</param>
public record PublicSignOutResponse(bool IsSuccess);

/// <summary>
/// Defines the sign-out endpoint for authenticated public users.
/// </summary>
public class PublicSignOutEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the sign-out route within the API pipeline.
    /// Maps the <c>POST /api/v1/public/auth/sign-out</c> endpoint to handle sign-out requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{IdentityConstants.SchemaName}");

        group
            .MapPost(
                pattern: AuthRouteConstants.SignOut,
                async (
                    PublicSignOutRequest? request,
                    ClaimsPrincipal user,
                    IClaimsProvider authProvider,
                    IDispatcher dispatcher,
                    ITokenDeliveryService tokenDelivery
                ) =>
                {
                    Guid userId = authProvider.GetUserIdFromClaims(user: user);
                    string? refreshToken = tokenDelivery.ReadRefreshToken(bodyRefreshToken: request?.RefreshToken);

                    var command = new PublicSignOutCommand(UserId: userId, RefreshToken: refreshToken!);
                    PublicSignOutResult result = await dispatcher.Send(request: command);

                    if (tokenDelivery.IsWebClient())
                    {
                        tokenDelivery.ClearTokenCookies();
                    }

                    var response = new PublicSignOutResponse(IsSuccess: result.IsSuccess);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: PublicSignOutMetaField.SignOut.Name)
            .WithSummary(summary: PublicSignOutMetaField.SignOut.Summary)
            .WithDescription(description: PublicSignOutMetaField.SignOut.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.SessionManagement)
            .Produces<PublicSignOutResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
