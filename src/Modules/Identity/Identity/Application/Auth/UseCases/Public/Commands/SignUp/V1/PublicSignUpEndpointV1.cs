using _116.BuildingBlocks.Constants.RateLimit;
using _116.BuildingBlocks.Utils;
using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.User.Constants;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp.V1;

/// <summary>
/// Request model for public user signup.
/// </summary>
/// <param name="Email">The user's email address for account verification.</param>
/// <param name="UserName">The desired username (alphanumeric with spaces and hyphens allowed).</param>
/// <param name="Password">The user's password in plain text format (will be hashed).</param>
public record PublicSignUpRequest(string Email, string UserName, string Password);

/// <summary>
/// Response model for public user signup, deliberately carrying no tokens and setting no cookies.
/// </summary>
/// <param name="User">The created user information.</param>
/// <param name="VerificationRequired">Indicates that email verification must happen before login.</param>
public record PublicSignUpResponse(UserResponseDto User, bool VerificationRequired);

/// <summary>
/// Defines the public signup endpoint for new user registration, directing the user to email
/// verification before their first login.
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
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{IdentityConstants.SchemaName}");

        group
            .MapPost(
                pattern: AuthRouteConstants.SignUp,
                async (PublicSignUpRequest request, IDispatcher dispatcher, HttpContext httpContext) =>
                {
                    var command = new PublicSignUpCommand(
                        Email: request.Email,
                        UserName: request.UserName,
                        Password: request.Password
                    );

                    PublicSignUpResult result = await dispatcher.Send(request: command);

                    string userPath = $"{IdentityConstants.Public}/{UserRouteConstants.Endpoint}/{result.User.Id}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: userPath);

                    var response = new PublicSignUpResponse(
                        User: result.User,
                        VerificationRequired: result.VerificationRequired
                    );

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: PublicSignUpMetaField.SignUp.Name)
            .WithSummary(summary: PublicSignUpMetaField.SignUp.Summary)
            .WithDescription(description: PublicSignUpMetaField.SignUp.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.Authentication)
            .ProducesValidationProblem()
            .Produces<PublicSignUpResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
