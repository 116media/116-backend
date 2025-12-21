using _116.Identity.Application.Shared.Constants;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;

using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.Admin.UseCases.Commands.Login.V1;

/// <summary>
/// Request model for admin authentication.
/// </summary>
/// <param name="Email">The admin's email address.</param>
/// <param name="Password">The admin's password.</param>
public record AdminLoginRequest(
    string Email,
    string Password
);
/// <summary>
/// Response model for successful admin authentication.
/// </summary>
/// <param name="User">The authenticated admin user information.</param>
/// <param name="AccessToken">The JWT access token with admin claims.</param>
/// <param name="AccessTokenExpiresAt">Date and time when the access token expires in UTC.</param>
/// <param name="RefreshToken">Refresh token for obtaining new access tokens.</param>
/// <param name="RefreshTokenExpiresAt">Date and time when the refresh token expires in UTC.</param>
/// <param name="TokenType">Type of token (typically "Bearer").</param>
public record AdminLoginResponse(
    UserResponseDto User,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    string TokenType
);
/// <summary>
/// Defines the admin login endpoint for authentication (V1).
/// Handles the process of validating credentials, issuing a JWT token,
/// and returning the authenticated admin's profile details.
/// </summary>
public class AdminLoginEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin login route within the API pipeline.
    /// Maps the <c>/api/v1/admin/auth/login</c> endpoint to handle authentication requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{IdentityConstants.Admin}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{IdentityConstants.SchemaName}");
        group.MapPost(AuthRouteConstants.Login, async (AdminLoginRequest request, IDispatcher dispatcher) =>
            {
                // Send the command to log in the admin
                var command = new AdminLoginCommand(request.Email, request.Password);
                AdminLoginResult result = await dispatcher.Send(command);
                // Adapt the result to the response type
                var response = new AdminLoginResponse(
                    result.AuthenticationResult.User,
                    result.AuthenticationResult.AccessToken,
                    result.AuthenticationResult.AccessTokenExpiresAt,
                    result.AuthenticationResult.RefreshToken,
                    result.AuthenticationResult.RefreshTokenExpiresAt,
                    result.AuthenticationResult.TokenType
                );
                return Results.Ok(response);
            })
            .WithName(AdminLoginMetaField.AdminLogin.Name)
            .WithSummary(AdminLoginMetaField.AdminLogin.Summary)
            .WithDescription(AdminLoginMetaField.AdminLogin.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<AdminLoginResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}
