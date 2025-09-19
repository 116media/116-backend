using _116.BuildingBlocks.Constants;
using Carter;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.User.Application.Admin.UseCases.Commands.VerifyOtp;

/// <summary>
/// Request model for admin OTP verification.
/// </summary>
/// <param name="Email">The admin user's email address.</param>
/// <param name="Code">The OTP code to verify.</param>
public record AdminVerifyOtpRequest(
    string Email,
    string Code
);

/// <summary>
/// Response model for successful admin OTP verification.
/// </summary>
/// <param name="IsSuccess">Indicates whether the verification was successful.</param>
public record AdminVerifyOtpResponse(
    bool IsSuccess
);

/// <summary>
/// Defines the admin OTP verification endpoint for admin account verification.
/// Handles OTP code validation and account activation.
/// </summary>
public class AdminVerifyOtpEndpoint : ICarterModule
{
    /// <summary>
    /// Configures the admin OTP verification route within the API pipeline.
    /// Maps the <c>/api/v1/admin/auth/verify-otp</c> endpoint to handle verification requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup(RouteConstants.V1.Admin.Auth)
            .WithTags("Admin::authentication");

        group.MapPost("/verify-otp", async (AdminVerifyOtpRequest request, IDispatcher dispatcher) =>
            {
                // Send the command to verify the OTP
                var command = new AdminVerifyOtpCommand(request.Email, request.Code);
                AdminVerifyOtpResult result = await dispatcher.Send(command);

                // Adapt the result to the response type
                var response = new AdminVerifyOtpResponse(
                    result.IsSuccess
                );

                return Results.Ok(response);
            })
            .WithName(AdminVerifyOtpMetaField.VerifyOtp.Name)
            .WithSummary(AdminVerifyOtpMetaField.VerifyOtp.Summary)
            .WithDescription(AdminVerifyOtpMetaField.VerifyOtp.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<AdminVerifyOtpResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
