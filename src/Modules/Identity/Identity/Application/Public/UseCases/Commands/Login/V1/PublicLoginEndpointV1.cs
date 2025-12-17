using _116.Auth.Application.Shared.Constants;
using _116.Auth.Domain.Constants;
using _116.Shared.Contracts.Application.CQRS;
using _116.Auth.Domain.DTOs;
using _116.Shared.Application.Extensions;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Auth.Application.Public.UseCases.Commands.Login.V1;

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
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{AuthConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{AuthConstants.Public}::{AuthConstants.SchemaName}");

        group.MapPost(AuthRouteConstants.Login, async (PublicLoginRequest request, IDispatcher dispatcher) =>
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
