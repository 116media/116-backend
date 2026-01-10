using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.Login.V1;

/// <summary>
/// Request model for admin authentication.
/// </summary>
/// <param name="Email">The admin's email address.</param>
/// <param name="Password">The admin's password.</param>
public record AdminLoginRequest(string Email, string Password);

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
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{IdentityConstants.SchemaName}");

        group
            .MapPost(
                pattern: AuthRouteConstants.Login,
                async (AdminLoginRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminLoginCommand(Email: request.Email, Password: request.Password);
                    AdminLoginResult result = await dispatcher.Send(request: command);

                    var response = new AdminLoginResponse(
                        User: result.AuthenticationResult.User,
                        AccessToken: result.AuthenticationResult.AccessToken,
                        AccessTokenExpiresAt: result.AuthenticationResult.AccessTokenExpiresAt,
                        RefreshToken: result.AuthenticationResult.RefreshToken,
                        RefreshTokenExpiresAt: result.AuthenticationResult.RefreshTokenExpiresAt,
                        TokenType: result.AuthenticationResult.TokenType
                    );

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminLoginMetaField.AdminLogin.Name)
            .WithSummary(summary: AdminLoginMetaField.AdminLogin.Summary)
            .WithDescription(description: AdminLoginMetaField.AdminLogin.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.Authentication)
            .ProducesValidationProblem()
            .Produces<AdminLoginResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests)
            .Produces(statusCode: StatusCodes.Status404NotFound);
    }
}
