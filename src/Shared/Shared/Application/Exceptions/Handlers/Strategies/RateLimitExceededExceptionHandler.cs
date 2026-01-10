using _116.Shared.Application.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Shared.Application.Exceptions.Handlers.Strategies;

/// <summary>
/// Strategy for handling RateLimitExceededException instances.
/// </summary>
public sealed class RateLimitExceededExceptionHandler : BaseExceptionStrategy<RateLimitExceededException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(RateLimitExceededException exception, HttpContext context)
    {
        context.Response.Headers.RetryAfter = ((int)exception.RetryAfter.TotalSeconds).ToString();

        return CreateStandardProblemDetails(
            title: nameof(RateLimitExceededException),
            detail: exception.Message,
            statusCode: StatusCodes.Status429TooManyRequests,
            context: context
        );
    }
}
