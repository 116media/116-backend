using _116.Shared.Application.Exceptions.Handlers.Contracts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Identity.Application.Auth.Exceptions.Handlers;

/// <summary>
/// Strategy for handling <see cref="OtpAttemptsLimitException" /> instances.
/// </summary>
public sealed class OtpAttemptsLimitExceptionHandler : BaseExceptionStrategy<OtpAttemptsLimitException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(OtpAttemptsLimitException exception, HttpContext context)
    {
        return CreateStandardProblemDetails(
            nameof(OtpAttemptsLimitException),
            detail: exception.Message,
            statusCode: StatusCodes.Status429TooManyRequests,
            context: context
        );
    }
}
