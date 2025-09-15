using _116.BuildingBlocks.Constants;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.User.Application.Public.UseCases.Commands.ForgotPassword;

/// <summary>
/// Request model for forgot password.
/// </summary>
/// <param name="Email">The user's registered email address.</param>
public record PublicForgotPasswordRequest(
    string Email
);

/// <summary>
/// Response model for forgot password.
/// </summary>
/// <param name="IsSuccess">Always true for security reasons to prevent user enumeration.</param>
public record PublicForgotPasswordResponse(
    bool IsSuccess
);

/// <summary>
/// Defines the forgot password endpoint for initiating password reset.
/// </summary>
public class PublicForgotPasswordEndpoint : ICarterModule
{
    /// <summary>
    /// Configures the forgot password route within the API pipeline.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup(RouteConstants.V1.Public.Auth)
            .WithTags("Public::authentication");

        group.MapPost("/forgot-password", async (PublicForgotPasswordRequest request, IDispatcher dispatcher) =>
            {
                var command = new PublicForgotPasswordCommand(request.Email);
                PublicForgotPasswordResult result = await dispatcher.Send(command);

                var response = new PublicForgotPasswordResponse(result.IsSuccess);

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
