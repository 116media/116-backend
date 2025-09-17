using _116.BuildingBlocks.Constants;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.User.Application.Public.UseCases.Commands.ResetPassword;

/// <summary>
/// Request model for password reset.
/// </summary>
/// <param name="Email">The user's registered email address.</param>
/// <param name="Code">The OTP code received for password reset.</param>
/// <param name="NewPassword">The new password to set for the user.</param>
public record PublicResetPasswordRequest(
    string Email,
    string Code,
    string NewPassword
);

/// <summary>
/// Response model for password reset.
/// </summary>
/// <param name="IsSuccess">Indicates whether the password reset was successful.</param>
public record PublicResetPasswordResponse(
    bool IsSuccess
);

/// <summary>
/// Defines the password reset endpoint for public users.
/// Handles password reset using OTP verification.
/// </summary>
public class PublicResetPasswordEndpoint : ICarterModule
{
    /// <summary>
    /// Configures the password reset route within the API pipeline.
    /// Maps the <c>/api/v1/public/auth/reset-password</c> endpoint to handle password reset requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup(RouteConstants.V1.Public.Auth)
            .WithTags("Public::authentication");

        group.MapPost("/reset-password", async (PublicResetPasswordRequest request, IDispatcher dispatcher) =>
            {
                // Send the command to reset the password
                var command = new PublicResetPasswordCommand(
                    request.Email,
                    request.Code,
                    request.NewPassword
                );

                PublicResetPasswordResult result = await dispatcher.Send(command);

                // Adapt the result to the response type
                var response = new PublicResetPasswordResponse(
                    result.IsSuccess
                );

                return Results.Ok(response);
            })
            .WithName(PublicResetPasswordMetaField.ResetPassword.Name)
            .WithSummary(PublicResetPasswordMetaField.ResetPassword.Summary)
            .WithDescription(PublicResetPasswordMetaField.ResetPassword.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<PublicResetPasswordResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
