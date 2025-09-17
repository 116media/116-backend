using _116.BuildingBlocks.Constants;
using _116.Shared.Contracts.Application.CQRS;
using _116.User.Domain.DTOs;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.User.Application.Public.UseCases.Commands.Login;

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
/// <param name="Token">The JWT access token.</param>
public record PublicLoginResponse(
    UserResponseDto User,
    string Token
);

/// <summary>
/// Defines the public login endpoint for user authentication.
/// Handles credential validation, token issuance,
/// and returns the authenticated user’s profile details.
/// </summary>
public class PublicLoginEndpoint : ICarterModule
{
    /// <summary>
    /// Configures the public login route within the API pipeline.
    /// Maps the <c>/api/v1/public/auth/login</c> endpoint to handle authentication requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup(RouteConstants.V1.Public.Auth)
            .WithTags("Public::authentication");

        group.MapPost("/login", async (PublicLoginRequest request, IDispatcher dispatcher) =>
            {
                // Send the command to log in the public user
                var command = new PublicLoginCommand(request.Credentials, request.Password);
                PublicLoginResult result = await dispatcher.Send(command);

                // Adapt the result to the response type
                var response = new PublicLoginResponse(
                    result.AuthenticationResult.User,
                    result.AuthenticationResult.Token
                );

                return Results.Ok(response);
            })
            .WithName(PublicLoginMetaField.PublicLogin.Name)
            .WithSummary(PublicLoginMetaField.PublicLogin.Summary)
            .WithDescription(PublicLoginMetaField.PublicLogin.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<PublicLoginResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}
