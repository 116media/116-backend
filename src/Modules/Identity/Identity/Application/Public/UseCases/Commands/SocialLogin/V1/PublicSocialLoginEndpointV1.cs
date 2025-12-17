using _116.Auth.Application.Shared.Constants;
using _116.Auth.Domain.Constants;
using _116.Auth.Domain.DTOs;
using _116.Shared.Application.Extensions;
using Carter;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Auth.Application.Public.UseCases.Commands.SocialLogin.V1;

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
/// <param name="Token">The JWT access token.</param>
public record PublicSocialLoginResponse(
    UserResponseDto User,
    string Token
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
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{AuthConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{AuthConstants.Public}::{AuthConstants.SchemaName}");

        group.MapPost(AuthRouteConstants.SocialLogin, async (
                PublicSocialLoginRequest request,
                IDispatcher dispatcher
            ) =>
            {
                // Send the command for social authentication
                var command = new PublicSocialLoginCommand(
                    Email: request.Email,
                    UserName: request.UserName,
                    AvatarUrl: request.AvatarUrl,
                    Provider: request.Provider
                );

                PublicSocialLoginResult result = await dispatcher.Send(command);

                // Create response
                var response = new PublicSocialLoginResponse(
                    result.AuthenticationResult.User,
                    result.AuthenticationResult.Token
                );

                return Results.Ok(response);
            })
            .WithName(PublicSocialLoginMetaField.SocialLogin.Name)
            .WithSummary(PublicSocialLoginMetaField.SocialLogin.Summary)
            .WithDescription(PublicSocialLoginMetaField.SocialLogin.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<PublicSocialLoginResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
