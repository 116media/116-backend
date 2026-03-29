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

namespace _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken.V1;

/// <summary>
/// Request model for refreshing an access token.
/// </summary>
/// <param name="RefreshToken">The refresh token to validate and rotate.</param>
public record PublicRefreshTokenRequest(string RefreshToken);

/// <summary>
/// Response model for mobile client token refresh (tokens delivered in the body).
/// </summary>
/// <param name="User">The authenticated user information.</param>
/// <param name="AccessToken">The new JWT access token.</param>
/// <param name="AccessTokenExpiresAt">Date and time when the access token expires in UTC.</param>
/// <param name="RefreshToken">New refresh token (old token is invalidated).</param>
/// <param name="RefreshTokenExpiresAt">Date and time when the refresh token expires in UTC.</param>
/// <param name="TokenType">Type of token (typically "Bearer").</param>
public record PublicRefreshTokenMobileResponse(
    UserResponseDto User,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    string TokenType
);

/// <summary>
/// Response model for web client token refresh (tokens delivered via HttpOnly cookies).
/// </summary>
/// <param name="User">The authenticated user information.</param>
public record PublicRefreshTokenWebResponse(UserResponseDto User);

/// <summary>
/// Defines the public refresh token endpoint for obtaining new access tokens.
/// Handles refresh token validation, token rotation,
/// and returns new authentication credentials.
/// </summary>
public class PublicRefreshTokenEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the public refresh token route within the API pipeline.
    /// Maps the <c>/api/v1/public/sessions/refresh-token</c> endpoint to handle token refresh requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{SessionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{SessionRouteConstants.Endpoint}");

        group
            .MapPost(
                pattern: SessionRouteConstants.RefreshToken,
                async (
                    PublicRefreshTokenRequest? request,
                    IDispatcher dispatcher,
                    ITokenDeliveryService tokenDelivery
                ) =>
                {
                    string? refreshToken = tokenDelivery.ReadRefreshToken(bodyRefreshToken: request?.RefreshToken);
                    if (string.IsNullOrEmpty(refreshToken))
                    {
                        throw SessionErrors.InvalidRefreshToken();
                    }

                    var command = new PublicRefreshTokenCommand(RefreshToken: refreshToken);
                    PublicRefreshTokenResult result = await dispatcher.Send(request: command);

                    if (tokenDelivery.IsWebClient())
                    {
                        tokenDelivery.SetTokenCookies(authResult: result.AuthenticationResult);
                        var webResponse = new PublicRefreshTokenWebResponse(User: result.AuthenticationResult.User);
                        return Results.Ok(value: webResponse);
                    }

                    var mobileResponse = new PublicRefreshTokenMobileResponse(
                        User: result.AuthenticationResult.User,
                        TokenType: result.AuthenticationResult.TokenType,
                        AccessToken: result.AuthenticationResult.AccessToken,
                        RefreshToken: result.AuthenticationResult.RefreshToken,
                        AccessTokenExpiresAt: result.AuthenticationResult.AccessTokenExpiresAt,
                        RefreshTokenExpiresAt: result.AuthenticationResult.RefreshTokenExpiresAt
                    );

                    return Results.Ok(value: mobileResponse);
                }
            )
            .WithName(endpointName: PublicRefreshTokenMetaField.PublicRefreshToken.Name)
            .WithSummary(summary: PublicRefreshTokenMetaField.PublicRefreshToken.Summary)
            .WithDescription(description: PublicRefreshTokenMetaField.PublicRefreshToken.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.SessionManagement)
            .ProducesValidationProblem()
            .Produces<PublicRefreshTokenMobileResponse>()
            .Produces<PublicRefreshTokenWebResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
