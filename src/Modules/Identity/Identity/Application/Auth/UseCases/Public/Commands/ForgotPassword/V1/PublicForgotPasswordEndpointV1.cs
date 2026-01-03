using _116.Identity.Application.Auth.Constants;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;

using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword.V1;

/// <summary>
/// Request model for the forgot password use-case.
/// </summary>
/// <param name="Email">The user's registered email address.</param>
public record PublicForgotPasswordRequest(
    string Email
);

/// <summary>
/// Response model for the forgot password use-case.
/// </summary>
/// <param name="IsSuccess">Always true for security reasons to prevent user enumeration.</param>
/// <param name="Email">The email address from the request for client reference.</param>
public record PublicForgotPasswordResponse(
    bool IsSuccess,
    string Email
);

/// <summary>
/// Defines the forgot password endpoint for initiating password reset.
/// </summary>
public class PublicForgotPasswordEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the forgot password route within the API pipeline.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{IdentityConstants.SchemaName}");

        group.MapPost(pattern: AuthRouteConstants.ForgotPassword, async (
                PublicForgotPasswordRequest request,
                IDispatcher dispatcher
            ) =>
            {
                var command = new PublicForgotPasswordCommand(Email: request.Email);
                PublicForgotPasswordResult result = await dispatcher.Send(request: command);

                var response = new PublicForgotPasswordResponse(
                    IsSuccess: result.IsSuccess,
                    Email: request.Email
                );

                return Results.Ok(value: response);
            })
            .WithName(endpointName: PublicForgotPasswordMetaField.ForgotPassword.Name)
            .WithSummary(summary: PublicForgotPasswordMetaField.ForgotPassword.Summary)
            .WithDescription(description: PublicForgotPasswordMetaField.ForgotPassword.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<PublicForgotPasswordResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest);
    }
}
