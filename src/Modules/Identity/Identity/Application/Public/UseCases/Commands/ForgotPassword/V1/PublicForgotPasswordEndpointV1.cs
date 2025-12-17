using _116.Auth.Application.Shared.Constants;
using _116.Auth.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Auth.Application.Public.UseCases.Commands.ForgotPassword.V1;

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
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{AuthConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{AuthConstants.Public}::{AuthConstants.SchemaName}");

        group.MapPost(AuthRouteConstants.ForgotPassword, async (
                PublicForgotPasswordRequest request,
                IDispatcher dispatcher
            ) =>
            {
                var command = new PublicForgotPasswordCommand(request.Email);
                PublicForgotPasswordResult result = await dispatcher.Send(command);

                var response = new PublicForgotPasswordResponse(result.IsSuccess, request.Email);

                return Results.Ok(response);
            })
            .WithName(PublicForgotPasswordMetaField.ForgotPassword.Name)
            .WithSummary(PublicForgotPasswordMetaField.ForgotPassword.Summary)
            .WithDescription(PublicForgotPasswordMetaField.ForgotPassword.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<PublicForgotPasswordResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
