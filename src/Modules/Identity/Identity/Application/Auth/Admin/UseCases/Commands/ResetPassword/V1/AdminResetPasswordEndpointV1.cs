using _116.Identity.Application.Shared.Constants;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;

using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.Admin.UseCases.Commands.ResetPassword.V1;

/// <summary>
/// Request model for admin password reset.
/// </summary>
/// <param name="Email">The admin user's registered email address.</param>
/// <param name="Code">The OTP code received for password reset.</param>
/// <param name="NewPassword">The new password to set for the admin user.</param>
public record AdminResetPasswordRequest(
    string Email,
    string Code,
    string NewPassword
);
/// <summary>
/// Response model for admin password reset.
/// </summary>
/// <param name="IsSuccess">Indicates whether the password reset was successful.</param>
public record AdminResetPasswordResponse(
    bool IsSuccess
);
/// <summary>
/// Defines the password reset endpoint for admin users (V1).
/// Handles password reset using OTP verification.
/// </summary>
public class AdminResetPasswordEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin password reset route within the API pipeline.
    /// Maps the <c>/api/v1/admin/auth/reset-password</c> endpoint to handle password reset requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{IdentityConstants.Admin}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{IdentityConstants.SchemaName}");
        group.MapPost(AuthRouteConstants.ResetPassword, async (
                AdminResetPasswordRequest request,
                IDispatcher dispatcher
            ) =>
            {
                // Send the command to reset the password
                var command = new AdminResetPasswordCommand(
                    request.Email,
                    request.Code,
                    request.NewPassword
                );
                AdminResetPasswordResult result = await dispatcher.Send(command);
                // Adapt the result to the response type
                var response = new AdminResetPasswordResponse(
                    result.IsSuccess
                );
                return Results.Ok(response);
            })
            .WithName(AdminResetPasswordMetaField.ResetPassword.Name)
            .WithSummary(AdminResetPasswordMetaField.ResetPassword.Summary)
            .WithDescription(AdminResetPasswordMetaField.ResetPassword.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<AdminResetPasswordResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
