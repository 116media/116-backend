using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Shared.DTOs;
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
/// Response model for successful token refresh.
/// </summary>
/// <param name="User">The authenticated user information.</param>
/// <param name="AccessToken">The new JWT access token.</param>
/// <param name="AccessTokenExpiresAt">Date and time when the access token expires in UTC.</param>
/// <param name="RefreshToken">New refresh token (old token is invalidated).</param>
/// <param name="RefreshTokenExpiresAt">Date and time when the refresh token expires in UTC.</param>
/// <param name="TokenType">Type of token (typically "Bearer").</param>
public record PublicRefreshTokenResponse(
    UserResponseDto User,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    string TokenType
);

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
        RouteGroupBuilder group = app
            .MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{SessionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{SessionRouteConstants.Endpoint}");

        group.MapPost(pattern: SessionRouteConstants.RefreshToken, async (
                PublicRefreshTokenRequest request,
                IDispatcher dispatcher
            ) =>
            {
                var command = new PublicRefreshTokenCommand(RefreshToken: request.RefreshToken);
                PublicRefreshTokenResult result = await dispatcher.Send(request: command);

                var response = new PublicRefreshTokenResponse(
                    User: result.AuthenticationResult.User,
                    TokenType: result.AuthenticationResult.TokenType,
                    AccessToken: result.AuthenticationResult.AccessToken,
                    RefreshToken: result.AuthenticationResult.RefreshToken,
                    AccessTokenExpiresAt: result.AuthenticationResult.AccessTokenExpiresAt,
                    RefreshTokenExpiresAt: result.AuthenticationResult.RefreshTokenExpiresAt
                );

                return Results.Ok(value: response);
            })
            .WithName(endpointName: PublicRefreshTokenMetaField.PublicRefreshToken.Name)
            .WithSummary(summary: PublicRefreshTokenMetaField.PublicRefreshToken.Summary)
            .WithDescription(description: PublicRefreshTokenMetaField.PublicRefreshToken.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<PublicRefreshTokenResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden);
    }
}
