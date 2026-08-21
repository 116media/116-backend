using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.V1;

/// <summary>
/// Request model for social login authentication. The client sends only the provider and the
/// provider-issued token; identity (email, name, avatar) is read from the verified token, never from
/// the client.
/// </summary>
/// <param name="Provider">The social authentication provider (Google or Facebook).</param>
/// <param name="IdToken">The provider-issued ID / access token to verify.</param>
public record PublicSocialLoginRequest(string Provider, string IdToken);

/// <summary>
/// Response model for mobile client social login (tokens delivered in the body).
/// </summary>
/// <param name="User">The authenticated user information.</param>
/// <param name="AccessToken">The JWT access token.</param>
/// <param name="AccessTokenExpiresAt">Date and time when the access token expires in UTC.</param>
/// <param name="RefreshToken">Refresh token for obtaining new access tokens.</param>
/// <param name="RefreshTokenExpiresAt">Date and time when the refresh token expires in UTC.</param>
/// <param name="TokenType">Type of token (typically "Bearer").</param>
public record PublicSocialLoginMobileResponse(
    UserResponseDto User,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    string TokenType
);

/// <summary>
/// Response model for web client social login (tokens delivered via HttpOnly cookies).
/// </summary>
/// <param name="User">The authenticated user information.</param>
public record PublicSocialLoginWebResponse(UserResponseDto User);

/// <summary>
/// Defines the social login endpoint for external provider authentication.
/// Handles Google and Facebook authentication with automatic user creation/update.
/// </summary>
public class PublicSocialLoginEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the social login route within the API pipeline.
    /// Maps the <c>/api/v1/public/auth/social-login</c> endpoint to handle social authentication.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{IdentityConstants.SchemaName}");

        group
            .MapPost(
                pattern: AuthRouteConstants.SocialLogin,
                async (PublicSocialLoginRequest request, IDispatcher dispatcher, ITokenDeliveryService tokenDelivery) =>
                {
                    var command = new PublicSocialLoginCommand(Provider: request.Provider, IdToken: request.IdToken);
                    PublicSocialLoginResult result = await dispatcher.Send(request: command);

                    if (tokenDelivery.IsWebClient())
                    {
                        tokenDelivery.SetTokenCookies(authResult: result.Authentication);
                        var webResponse = new PublicSocialLoginWebResponse(User: result.Authentication.User);
                        return Results.Ok(value: webResponse);
                    }

                    var mobileResponse = new PublicSocialLoginMobileResponse(
                        User: result.Authentication.User,
                        AccessToken: result.Authentication.AccessToken,
                        AccessTokenExpiresAt: result.Authentication.AccessTokenExpiresAt,
                        RefreshToken: result.Authentication.RefreshToken,
                        RefreshTokenExpiresAt: result.Authentication.RefreshTokenExpiresAt,
                        TokenType: result.Authentication.TokenType
                    );

                    return Results.Ok(value: mobileResponse);
                }
            )
            .WithName(endpointName: PublicSocialLoginMetaField.SocialLogin.Name)
            .WithSummary(summary: PublicSocialLoginMetaField.SocialLogin.Summary)
            .WithDescription(description: PublicSocialLoginMetaField.SocialLogin.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.Authentication)
            .ProducesValidationProblem()
            .Produces<PublicSocialLoginMobileResponse>()
            .Produces<PublicSocialLoginWebResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
