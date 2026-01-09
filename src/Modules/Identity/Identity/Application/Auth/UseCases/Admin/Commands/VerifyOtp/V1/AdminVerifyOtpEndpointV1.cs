using _116.Identity.Application.Auth.Constants;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp.V1;

/// <summary>
/// Request model for admin OTP verification.
/// </summary>
/// <param name="Email">The admin user's email address.</param>
/// <param name="Code">The OTP code to verify.</param>
/// <param name="Purpose">The purpose for which the OTP is being verified (EmailVerification or AccountRecovery).</param>
public record AdminVerifyOtpRequest(string Email, string Code, string Purpose);

/// <summary>
/// Response model for successful admin OTP verification.
/// </summary>
/// <param name="IsSuccess">Indicates whether the verification was successful.</param>
public record AdminVerifyOtpResponse(bool IsSuccess);

/// <summary>
/// Defines the admin OTP verification endpoint for admin account verification (V1).
/// Handles OTP code validation and account activation.
/// </summary>
public class AdminVerifyOtpEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin OTP verification route within the API pipeline.
    /// Maps the <c>/api/v1/admin/auth/verify-otp</c> endpoint to handle verification requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{IdentityConstants.SchemaName}");

        group
            .MapPost(
                pattern: AuthRouteConstants.VerifyOtp,
                async (AdminVerifyOtpRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminVerifyOtpCommand(
                        Email: request.Email,
                        Code: request.Code,
                        Purpose: request.Purpose
                    );
                    AdminVerifyOtpResult result = await dispatcher.Send(request: command);

                    var response = new AdminVerifyOtpResponse(IsSuccess: result.IsSuccess);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminVerifyOtpMetaField.VerifyOtp.Name)
            .WithSummary(summary: AdminVerifyOtpMetaField.VerifyOtp.Summary)
            .WithDescription(description: AdminVerifyOtpMetaField.VerifyOtp.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<AdminVerifyOtpResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden);
    }
}
