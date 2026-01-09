using _116.Shared.Application.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Identity.Application.Auth.Exceptions.Handlers;

/// <summary>
/// Strategy for handling <see cref="OtpExpirationException" /> instances.
/// </summary>
public sealed class OtpExpirationExceptionHandler : BaseExceptionStrategy<OtpExpirationException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(OtpExpirationException exception, HttpContext context)
    {
        return CreateStandardProblemDetails(
            nameof(OtpExpirationException),
            detail: exception.Message,
            statusCode: StatusCodes.Status410Gone,
            context: context
        );
    }
}
