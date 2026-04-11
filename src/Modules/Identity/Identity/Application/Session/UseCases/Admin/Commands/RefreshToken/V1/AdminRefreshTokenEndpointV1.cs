using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Session.UseCases.Admin.Commands.RefreshToken.V1;

/// <summary>
/// Response model for admin token refresh (tokens delivered via HttpOnly cookies).
/// </summary>
/// <param name="User">The authenticated admin user information.</param>
public record AdminRefreshTokenResponse(UserResponseDto User);

/// <summary>
/// Defines the admin refresh token endpoint for obtaining new access tokens.
/// Handles refresh token validation, token rotation,
/// and returns the authenticated admin's profile details.
/// </summary>
public class AdminRefreshTokenEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin refresh token route within the API pipeline.
    /// Maps the <c>/api/v1/admin/sessions/refresh-token</c> endpoint to handle token refresh requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{SessionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{SessionRouteConstants.Endpoint}");

        group
            .MapPost(
                pattern: SessionRouteConstants.RefreshToken,
                async (IDispatcher dispatcher, ITokenDeliveryService tokenDelivery) =>
                {
                    string? refreshToken = tokenDelivery.ReadRefreshToken(bodyRefreshToken: null);
                    if (string.IsNullOrEmpty(refreshToken))
                    {
                        throw SessionErrors.InvalidRefreshToken();
                    }

                    var command = new AdminRefreshTokenCommand(RefreshToken: refreshToken);
                    AdminRefreshTokenResult result = await dispatcher.Send(request: command);

                    tokenDelivery.SetTokenCookies(authResult: result.AuthenticationResult);

                    var response = new AdminRefreshTokenResponse(User: result.AuthenticationResult.User);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminRefreshTokenMetaField.AdminRefreshToken.Name)
            .WithSummary(summary: AdminRefreshTokenMetaField.AdminRefreshToken.Summary)
            .WithDescription(description: AdminRefreshTokenMetaField.AdminRefreshToken.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.SessionManagement)
            .ProducesValidationProblem()
            .Produces<AdminRefreshTokenResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
