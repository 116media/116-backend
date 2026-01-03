using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;

using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.Login.V1;

/// <summary>
/// Request model for public user authentication.
/// </summary>
/// <param name="Credentials">The user’s email or username.</param>
/// <param name="Password">The user’s password.</param>
public record PublicLoginRequest(
    string Credentials,
    string Password
);

/// <summary>
/// Response model for successful public user authentication.
/// </summary>
/// <param name="User">The authenticated user information.</param>
/// <param name="AccessToken">The JWT access token.</param>
/// <param name="AccessTokenExpiresAt">Date and time when the access token expires in UTC.</param>
/// <param name="RefreshToken">Refresh token for obtaining new access tokens.</param>
/// <param name="RefreshTokenExpiresAt">Date and time when the refresh token expires in UTC.</param>
/// <param name="TokenType">Type of token (typically "Bearer").</param>
public record PublicLoginResponse(
    UserResponseDto User,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    string TokenType
);

/// <summary>
/// Defines the public login endpoint for user authentication.
/// Handles credential validation, token issuance,
/// and returns the authenticated user’s profile details.
/// </summary>
public class PublicLoginEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the public login route within the API pipeline.
    /// Maps the <c>/api/v1/public/auth/login</c> endpoint to handle authentication requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{IdentityConstants.SchemaName}");

        group.MapPost(pattern: AuthRouteConstants.Login, async (PublicLoginRequest request, IDispatcher dispatcher) =>
            {
                var command = new PublicLoginCommand(Credentials: request.Credentials, Password: request.Password);
                PublicLoginResult result = await dispatcher.Send(request: command);

                var response = new PublicLoginResponse(
                    User: result.AuthenticationResult.User,
                    AccessToken: result.AuthenticationResult.AccessToken,
                    AccessTokenExpiresAt: result.AuthenticationResult.AccessTokenExpiresAt,
                    RefreshToken: result.AuthenticationResult.RefreshToken,
                    RefreshTokenExpiresAt: result.AuthenticationResult.RefreshTokenExpiresAt,
                    TokenType: result.AuthenticationResult.TokenType
                );

                return Results.Ok(value: response);
            })
            .WithName(endpointName: PublicLoginMetaField.PublicLogin.Name)
            .WithSummary(summary: PublicLoginMetaField.PublicLogin.Summary)
            .WithDescription(description: PublicLoginMetaField.PublicLogin.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<PublicLoginResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .Produces(statusCode: StatusCodes.Status404NotFound);
    }
}
