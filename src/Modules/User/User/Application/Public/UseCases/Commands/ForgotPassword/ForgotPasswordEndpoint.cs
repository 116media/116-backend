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
public record ForgotPasswordRequest(
    string Email
);

/// <summary>
/// Response model for forgot password.
/// </summary>
/// <param name="IsSuccess">Always true for security reasons to prevent user enumeration.</param>
public record ForgotPasswordResponse(
    bool IsSuccess
);

/// <summary>
/// Defines the forgot password endpoint for initiating password reset.
/// </summary>
public class ForgotPasswordEndpoint : ICarterModule
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

        group.MapPost("/forgot-password", async (ForgotPasswordRequest request, IDispatcher dispatcher) =>
            {
                var command = new ForgotPasswordCommand(request.Email);
                ForgotPasswordResult result = await dispatcher.Send(command);

                var response = new ForgotPasswordResponse(result.IsSuccess);

                return Results.Ok(response);
            })
            .WithName(ForgotPasswordMetaField.ForgotPassword.Name)
            .WithSummary(ForgotPasswordMetaField.ForgotPassword.Summary)
            .WithDescription(ForgotPasswordMetaField.ForgotPassword.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<ForgotPasswordResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
