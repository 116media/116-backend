using _116.BuildingBlocks.Constants;
using _116.Shared.Contracts.Application.CQRS;
using _116.User.Domain.Enums;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.User.Application.Admin.UseCases.Commands.ResendOtp;

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
/// Defines the admin resend OTP endpoint for generating new verification codes.
/// </summary>
public class AdminResendOtpEndpoint : ICarterModule
{
    /// <summary>
    /// Configures the admin resend OTP route within the API pipeline.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup(RouteConstants.V1.Admin.Auth)
            .WithTags("Admin::authentication");

        group.MapPost("/resend-otp", async (AdminResendOtpRequest request, IDispatcher dispatcher) =>
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