using _116.Identity.Application.Auth.Constants;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;

using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp.V1;

/// <summary>
/// Request model for OTP verification.
/// </summary>
/// <param name="Email">The user's email address.</param>
/// <param name="Code">The OTP code to verify.</param>
/// <param name="Purpose">The purpose for which the OTP is being verified (EmailVerification or AccountRecovery).</param>
public record PublicVerifyOtpRequest(
    string Email,
    string Code,
    string Purpose
);

/// <summary>
/// Response model for successful OTP verification.
/// </summary>
/// <param name="IsSuccess">Indicates whether the verification was successful.</param>
public record PublicVerifyOtpResponse(
    bool IsSuccess
);

/// <summary>
/// Defines the OTP verification endpoint for user account verification.
/// Handles OTP code validation and account activation.
/// </summary>
public class PublicVerifyOtpEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the OTP verification route within the API pipeline.
    /// Maps the <c>/api/v1/public/auth/verify-otp</c> endpoint to handle verification requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{IdentityConstants.SchemaName}");
        group.MapPost(pattern: AuthRouteConstants.VerifyOtp, async (
                PublicVerifyOtpRequest request,
                IDispatcher dispatcher
            ) =>
            {
                // Send the command to verify the OTP
                var command =
                    new PublicVerifyOtpCommand(Email: request.Email, Code: request.Code, Purpose: request.Purpose);
                PublicVerifyOtpResult result = await dispatcher.Send(request: command);
                // Adapt the result to the response type
                var response = new PublicVerifyOtpResponse(
                    IsSuccess: result.IsSuccess
                );
                return Results.Ok(value: response);
            })
            .WithName(endpointName: PublicVerifyOtpMetaField.VerifyOtp.Name)
            .WithSummary(summary: PublicVerifyOtpMetaField.VerifyOtp.Summary)
            .WithDescription(description: PublicVerifyOtpMetaField.VerifyOtp.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<PublicVerifyOtpResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden);
    }
}
