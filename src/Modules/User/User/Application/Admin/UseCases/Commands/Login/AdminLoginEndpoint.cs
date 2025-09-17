using _116.BuildingBlocks.Constants;
using _116.User.Domain.DTOs;
using Carter;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.User.Application.Admin.UseCases.Commands.Login;

// Request/Response DTOs for the Admin API module
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
/// <param name="Token">The JWT access token with admin claims.</param>
public record AdminLoginResponse(
    UserResponseDto User,
    string Token
);

/// <summary>
/// Defines the admin login endpoint for authentication.
/// Handles the process of validating credentials, issuing a JWT token,
/// and returning the authenticated admin’s profile details.
/// </summary>
public class AdminLoginEndpoint : ICarterModule
{
    /// <summary>
    /// Configures the admin login route within the API pipeline.
    /// Maps the <c>/api/v1/admin/auth/login</c> endpoint to handle authentication requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup(RouteConstants.V1.Admin.Auth)
            .WithTags("Admin::authentication");

        group.MapPost("/login", async (AdminLoginRequest request, IDispatcher dispatcher) =>
            {
                // Send the command to log in the admin
                var command = new AdminLoginCommand(request.Email, request.Password);
                AdminLoginResult result = await dispatcher.Send(command);

                // Adapt the result to the response type
                var response = new AdminLoginResponse(
                    result.AuthenticationResult.User,
                    result.AuthenticationResult.Token
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
