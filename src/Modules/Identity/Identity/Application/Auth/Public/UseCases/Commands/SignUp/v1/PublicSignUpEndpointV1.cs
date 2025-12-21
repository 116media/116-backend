using _116.BuildingBlocks.Utils;
using _116.Identity.Application.Shared.Constants;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;

using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.Public.UseCases.Commands.SignUp.v1;

/// <summary>
/// Request model for public user signup.
/// </summary>
/// <param name="Email">The user’s email address for account verification.</param>
/// <param name="UserName">The desired username (alphanumeric with spaces and hyphens allowed).</param>
/// <param name="Password">The user’s password in plain text format (will be hashed).</param>
public record PublicSignUpRequest(
    string Email,
    string UserName,
    string Password
);
/// <summary>
/// Response model for successful public user signup.
/// </summary>
/// <param name="User">The created user information.</param>
/// <param name="AccessToken">The JWT access token.</param>
/// <param name="AccessTokenExpiresAt">Date and time when the access token expires in UTC.</param>
/// <param name="RefreshToken">Refresh token for obtaining new access tokens.</param>
/// <param name="RefreshTokenExpiresAt">Date and time when the refresh token expires in UTC.</param>
/// <param name="TokenType">Type of token (typically "Bearer").</param>
/// <param name="VerificationRequired">Indicates whether the user must verify their email before full access.</param>
public record PublicSignUpResponse(
    UserResponseDto User,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    string TokenType,
    bool VerificationRequired
);
/// <summary>
/// Defines the public signup endpoint for new user registration.
/// Handles input validation, account creation, token issuance,
/// and indicates whether verification is required.
/// </summary>
public class PublicSignUpEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the public signup route within the API pipeline.
    /// Maps the <c>/api/v1/public/auth/signup</c> endpoint to handle registration requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{IdentityConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{IdentityConstants.SchemaName}");
        group.MapPost(AuthRouteConstants.SignUp, async (
                PublicSignUpRequest request,
                IDispatcher dispatcher,
                HttpContext httpContext
            ) =>
            {
                // Send the command to register the public user
                var command = new PublicSignUpCommand(
                    Email: request.Email,
                    UserName: request.UserName,
                    Password: request.Password
                );
                PublicSignUpResult result = await dispatcher.Send(command);
                // Adapt the result to the response type
                var response = new PublicSignUpResponse(
                    result.AuthenticationResult.User,
                    result.AuthenticationResult.AccessToken,
                    result.AuthenticationResult.AccessTokenExpiresAt,
                    result.AuthenticationResult.RefreshToken,
                    result.AuthenticationResult.RefreshTokenExpiresAt,
                    result.AuthenticationResult.TokenType,
                    result.VerificationRequired
                );
                string userPath = $"{IdentityConstants.Public}/{AuthRouteConstants.Users}/{response.User.Id}";
                string locationUrl = ApiVersionUrl.Build(httpContext, userPath);
                return Results.Created(locationUrl, response);
            })
            .WithName(PublicSignUpMetaField.PublicSignUp.Name)
            .WithSummary(PublicSignUpMetaField.PublicSignUp.Summary)
            .WithDescription(PublicSignUpMetaField.PublicSignUp.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<PublicSignUpResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
