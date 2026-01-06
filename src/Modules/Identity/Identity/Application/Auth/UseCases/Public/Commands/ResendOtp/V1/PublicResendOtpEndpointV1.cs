using _116.Identity.Application.Auth.Constants;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp.V1;

/// <summary>
/// Request model for public resend OTP.
/// </summary>
/// <param name="Email">The user's email address.</param>
/// <param name="Purpose">The purpose for which the OTP is being resent.</param>
public record PublicResendOtpRequest(string Email, string Purpose);

/// <summary>
/// Response model for public resend OTP.
/// </summary>
/// <param name="IsSuccess">Indicates whether the OTP was successfully resent.</param>
public record PublicResendOtpResponse(bool IsSuccess);

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
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Public}/{AuthRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Public}::{IdentityConstants.SchemaName}");

        group
            .MapPost(
                pattern: AuthRouteConstants.ResendOtp,
                async (PublicResendOtpRequest request, IDispatcher dispatcher) =>
                {
                    var command = new PublicResendOtpCommand(Email: request.Email, Purpose: request.Purpose);
                    PublicResendOtpResult result = await dispatcher.Send(request: command);

                    var response = new PublicResendOtpResponse(IsSuccess: result.IsSuccess);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: PublicResendOtpMetaField.ResendOtp.Name)
            .WithSummary(summary: PublicResendOtpMetaField.ResendOtp.Summary)
            .WithDescription(description: PublicResendOtpMetaField.ResendOtp.Description)
            .AllowAnonymous()
            .ProducesValidationProblem()
            .Produces<PublicResendOtpResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden);
    }
}
