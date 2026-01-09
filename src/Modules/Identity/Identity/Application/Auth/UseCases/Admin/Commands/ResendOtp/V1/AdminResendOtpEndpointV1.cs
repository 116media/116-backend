using _116.Identity.Application.Auth.Constants;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp.V1;

/// <summary>
/// Request model for admin resend OTP.
/// </summary>
/// <param name="Email">The admin user's email address.</param>
/// <param name="Purpose">The purpose for which the OTP is being resent.</param>
public record AdminResendOtpRequest(string Email, string Purpose);

/// <summary>
/// Response model for admin resend OTP.
/// </summary>
/// <param name="IsSuccess">Indicates whether the OTP was successfully resent.</param>
public record AdminResendOtpResponse(bool IsSuccess);

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
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{IdentityConstants.SchemaName}");

        group
            .MapPost(
                pattern: AuthRouteConstants.ResendOtp,
                async (AdminResendOtpRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminResendOtpCommand(Email: request.Email, Purpose: request.Purpose);
                    AdminResendOtpResult result = await dispatcher.Send(request: command);

                    var response = new AdminResendOtpResponse(IsSuccess: result.IsSuccess);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminResendOtpMetaField.ResendOtp.Name)
            .WithSummary(summary: AdminResendOtpMetaField.ResendOtp.Summary)
            .WithDescription(description: AdminResendOtpMetaField.ResendOtp.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<AdminResendOtpResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden);
    }
}
