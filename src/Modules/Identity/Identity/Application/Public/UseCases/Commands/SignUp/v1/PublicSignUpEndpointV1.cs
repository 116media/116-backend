using _116.Identity.Application.Shared.Constants;
using _116.Shared.Contracts.Application.CQRS;
using _116.Identity.Domain.Constants;
using _116.Identity.Domain.DTOs;
using _116.BuildingBlocks.Utils;
using _116.Shared.Application.Extensions;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Public.UseCases.Commands.SignUp.V1;

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
/// <param name="Token">The JWT access token.</param>
/// <param name="VerificationRequired">Indicates whether the user must verify their email before full access.</param>
public record PublicSignUpResponse(
    UserResponseDto User,
    string Token,
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
            .MapGroup($"{AuthConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{AuthConstants.Public}::{AuthConstants.SchemaName}");

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
                    result.AuthenticationResult.Token,
                    result.VerificationRequired
                );

                string userPath = $"{AuthConstants.Public}/{AuthRouteConstants.Users}/{response.User.Id}";
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
