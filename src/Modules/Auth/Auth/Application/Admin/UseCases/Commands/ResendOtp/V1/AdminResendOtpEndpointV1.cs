using _116.Auth.Application.Shared.Constants;
using _116.Auth.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Auth.Application.Admin.UseCases.Commands.ResendOtp.V1;

/// <summary>
/// Request model for admin resend OTP.
/// </summary>
/// <param name="Email">The admin user's email address.</param>
/// <param name="Purpose">The purpose for which the OTP is being resent.</param>
public record AdminResendOtpRequest(
    string Email,
    string Purpose
);

/// <summary>
/// Response model for admin resend OTP.
/// </summary>
/// <param name="IsSuccess">Indicates whether the OTP was successfully resent.</param>
public record AdminResendOtpResponse(
    bool IsSuccess
);

/// <summary>
/// Defines the admin resend OTP endpoint for generating new verification codes (V1).
/// </summary>
public class AdminResendOtpEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin resend OTP route within the API pipeline.
    /// Maps the <c>/api/v1/admin/auth/resend-otp</c> endpoint to handle OTP resend requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{AuthConstants.Admin}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{AuthConstants.Admin}::{AuthConstants.SchemaName}");

        group.MapPost(AuthRouteConstants.ResendOtp, async (
                AdminResendOtpRequest request,
                IDispatcher dispatcher
            ) =>
            {
                var command = new AdminResendOtpCommand(request.Email, request.Purpose);
                AdminResendOtpResult result = await dispatcher.Send(command);

                var response = new AdminResendOtpResponse(result.IsSuccess);

                return Results.Ok(response);
            })
            .WithName(AdminResendOtpMetaField.ResendOtp.Name)
            .WithSummary(AdminResendOtpMetaField.ResendOtp.Summary)
            .WithDescription(AdminResendOtpMetaField.ResendOtp.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<AdminResendOtpResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
