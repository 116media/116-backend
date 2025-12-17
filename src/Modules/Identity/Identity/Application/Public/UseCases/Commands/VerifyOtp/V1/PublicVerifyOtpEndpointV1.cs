using _116.Identity.Application.Shared.Constants;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using Carter;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Public.UseCases.Commands.VerifyOtp.V1;

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
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{AuthConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{AuthConstants.Public}::{AuthConstants.SchemaName}");

        group.MapPost(AuthRouteConstants.VerifyOtp, async (
                PublicVerifyOtpRequest request,
                IDispatcher dispatcher
            ) =>
            {
                // Send the command to verify the OTP
                var command = new PublicVerifyOtpCommand(request.Email, request.Code, request.Purpose);
                PublicVerifyOtpResult result = await dispatcher.Send(command);

                // Adapt the result to the response type
                var response = new PublicVerifyOtpResponse(
                    result.IsSuccess
                );

                return Results.Ok(response);
            })
            .WithName(PublicVerifyOtpMetaField.VerifyOtp.Name)
            .WithSummary(PublicVerifyOtpMetaField.VerifyOtp.Summary)
            .WithDescription(PublicVerifyOtpMetaField.VerifyOtp.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<PublicVerifyOtpResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
