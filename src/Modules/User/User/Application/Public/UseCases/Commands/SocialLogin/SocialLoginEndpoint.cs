using _116.BuildingBlocks.Constants;
using _116.User.Domain.DTOs;
using Carter;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.User.Application.Public.UseCases.Commands.SocialLogin;

/// <summary>
/// Request model for social login authentication.
/// </summary>
/// <param name="Email">The user's email address from the social provider.</param>
/// <param name="UserName">The user's display name from the social provider.</param>
/// <param name="Avatar">Optional avatar URL from the social provider.</param>
/// <param name="Provider">The social authentication provider (Google or Facebook).</param>
public record SocialLoginRequest(
    string Email,
    string UserName,
    string? Avatar,
    string Provider
);

/// <summary>
/// Response model for successful social login.
/// </summary>
/// <param name="User">The authenticated user information.</param>
/// <param name="Token">The JWT access token.</param>
public record SocialLoginResponse(
    UserResponseDto User,
    string Token
);

/// <summary>
/// Defines the social login endpoint for external provider authentication.
/// Handles Google and Facebook authentication with automatic user creation/update.
/// </summary>
public class SocialLoginEndpoint : ICarterModule
{
    /// <summary>
    /// Configures the social login route within the API pipeline.
    /// Maps the <c>/api/v1/public/auth/social-login</c> endpoint to handle social authentication.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup(RouteConstants.V1.Public.Auth)
            .WithTags("Public::authentication");

        group.MapPost("/social-login", async (SocialLoginRequest request, IDispatcher dispatcher) =>
            {
                // Send the command for social authentication
                var command = new SocialLoginCommand(
                    Email: request.Email,
                    UserName: request.UserName,
                    Avatar: request.Avatar,
                    Provider: request.Provider
                );

                SocialLoginResult result = await dispatcher.Send(command);

                // Create response
                var response = new SocialLoginResponse(
                    result.AuthenticationResult.User,
                    result.AuthenticationResult.Token
                );

                return Results.Ok(response);
            })
            .WithName(SocialLoginMetaField.SocialLogin.Name)
            .WithSummary(SocialLoginMetaField.SocialLogin.Summary)
            .WithDescription(SocialLoginMetaField.SocialLogin.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<SocialLoginResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
