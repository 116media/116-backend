using _116.Identity.Application.Shared.Constants;
using _116.Identity.Domain.Constants;
using _116.Shared.Contracts.Application.CQRS;
using _116.Shared.Application.Extensions;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Public.UseCases.Commands.ResendOtp.V1;

/// <summary>
/// Request model for public resend OTP.
/// </summary>
/// <param name="Email">The user's email address.</param>
/// <param name="Purpose">The purpose for which the OTP is being resent.</param>
public record PublicResendOtpRequest(
    string Email,
    string Purpose
);

/// <summary>
/// Response model for public resend OTP.
/// </summary>
/// <param name="IsSuccess">Indicates whether the OTP was successfully resent.</param>
public record PublicResendOtpResponse(
    bool IsSuccess
);

/// <summary>
/// Defines the public resend OTP endpoint for generating new verification codes.
/// </summary>
public class PublicResendOtpEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the public resend OTP route within the API pipeline.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(version: 1)
            .MapGroup($"{AuthConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{AuthConstants.Public}::{AuthConstants.SchemaName}");

        group.MapPost(AuthRouteConstants.ResendOtp, async (
                PublicResendOtpRequest request,
                IDispatcher dispatcher
            ) =>
            {
                var command = new PublicResendOtpCommand(request.Email, request.Purpose);
                PublicResendOtpResult result = await dispatcher.Send(command);

                var response = new PublicResendOtpResponse(result.IsSuccess);

                return Results.Ok(response);
            })
            .WithName(PublicResendOtpMetaField.ResendOtp.Name)
            .WithSummary(PublicResendOtpMetaField.ResendOtp.Summary)
            .WithDescription(PublicResendOtpMetaField.ResendOtp.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<PublicResendOtpResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
