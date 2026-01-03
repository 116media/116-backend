using _116.Identity.Application.Auth.Constants;
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
/// Request model for social login authentication.
/// </summary>
/// <param name="Email">The user's email address from the social provider.</param>
/// <param name="UserName">The user's display name from the social provider.</param>
/// <param name="AvatarUrl">Optional avatar URL from the social provider.</param>
/// <param name="Provider">The social authentication provider (Google or Facebook).</param>
public record PublicSocialLoginRequest(
    string Email,
    string UserName,
    string? AvatarUrl,
    string Provider
);

/// <summary>
/// Response model for successful social login.
/// </summary>
/// <param name="User">The authenticated user information.</param>
/// <param name="AccessToken">The JWT access token.</param>
/// <param name="AccessTokenExpiresAt">Date and time when the access token expires in UTC.</param>
/// <param name="RefreshToken">Refresh token for obtaining new access tokens.</param>
/// <param name="RefreshTokenExpiresAt">Date and time when the refresh token expires in UTC.</param>
/// <param name="TokenType">Type of token (typically "Bearer").</param>
public record PublicSocialLoginResponse(
    UserResponseDto User,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    string TokenType
);

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
        RouteGroupBuilder group = app
            .MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{IdentityConstants.SchemaName}");

        group.MapPost(pattern: AuthRouteConstants.SocialLogin, async (
                PublicSocialLoginRequest request,
                IDispatcher dispatcher
            ) =>
            {
                var command = new PublicSocialLoginCommand(
                    Email: request.Email,
                    UserName: request.UserName,
                    AvatarUrl: request.AvatarUrl,
                    Provider: request.Provider
                );
                PublicSocialLoginResult result = await dispatcher.Send(request: command);

                var response = new PublicSocialLoginResponse(
                    User: result.AuthenticationResult.User,
                    AccessToken: result.AuthenticationResult.AccessToken,
                    AccessTokenExpiresAt: result.AuthenticationResult.AccessTokenExpiresAt,
                    RefreshToken: result.AuthenticationResult.RefreshToken,
                    RefreshTokenExpiresAt: result.AuthenticationResult.RefreshTokenExpiresAt,
                    TokenType: result.AuthenticationResult.TokenType
                );

                return Results.Ok(value: response);
            })
            .WithName(endpointName: PublicSocialLoginMetaField.SocialLogin.Name)
            .WithSummary(summary: PublicSocialLoginMetaField.SocialLogin.Summary)
            .WithDescription(description: PublicSocialLoginMetaField.SocialLogin.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<PublicSocialLoginResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict);
    }
}
