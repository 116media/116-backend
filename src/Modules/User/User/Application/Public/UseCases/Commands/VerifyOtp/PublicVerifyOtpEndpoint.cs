using _116.BuildingBlocks.Constants;
using Carter;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.User.Application.Public.UseCases.Commands.VerifyOtp;

/// <summary>
/// Request model for OTP verification.
/// </summary>
/// <param name="Email">The user's email address.</param>
/// <param name="Code">The OTP code to verify.</param>
public record PublicVerifyOtpRequest(
    string Email,
    string Code
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
public class PublicVerifyOtpEndpoint : ICarterModule
{
    /// <summary>
    /// Configures the OTP verification route within the API pipeline.
    /// Maps the <c>/api/v1/public/auth/verify-otp</c> endpoint to handle verification requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup(RouteConstants.V1.Public.Auth)
            .WithTags("Public::authentication");

        group.MapPost("/verify-otp", async (PublicVerifyOtpRequest request, IDispatcher dispatcher) =>
            {
                // Send the command to verify the OTP
                var command = new PublicVerifyOtpCommand(request.Email, request.Code);
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
