using _116.Identity.Application.Auth.Constants;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;

using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword.V1;

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
/// NOTE: This is the LEGACY endpoint using RouteConstants.V1.Public.Auth
/// For new versioning approach, see V1/PublicResetPasswordV1Endpoint.cs and V2/PublicResetPasswordV2Endpoint.cs
/// </summary>
public class PublicResetPasswordEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the password reset route within the API pipeline.
    /// Maps the <c>/api/v1/public/auth/reset-password</c> endpoint to handle password reset requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{IdentityConstants.SchemaName}");

        group.MapPost(pattern: AuthRouteConstants.ResetPassword, async (
                PublicResetPasswordRequest request,
                IDispatcher dispatcher
            ) =>
            {
                var command = new PublicResetPasswordCommand(
                    Email: request.Email,
                    Code: request.Code,
                    NewPassword: request.NewPassword
                );
                PublicResetPasswordResult result = await dispatcher.Send(request: command);

                var response = new PublicResetPasswordResponse(IsSuccess: result.IsSuccess);

                return Results.Ok(value: response);
            })
            .WithName(endpointName: PublicResetPasswordMetaField.ResetPassword.Name)
            .WithSummary(summary: PublicResetPasswordMetaField.ResetPassword.Summary)
            .WithDescription(description: PublicResetPasswordMetaField.ResetPassword.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<PublicResetPasswordResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden);
    }
}
