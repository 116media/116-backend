using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Application.Exceptions.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

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

        string detail;
        if (exception.HasCustomMessage)
        {
            detail = exception.Message;
        }
        else
        {
            var msg = context.RequestServices.GetRequiredService<SharedExceptionMessage>();
            detail = msg.RateLimitExceeded((int)exception.RetryAfter.TotalSeconds);
        }

        return CreateStandardProblemDetails(
            title: nameof(RateLimitExceededException),
            detail: detail,
            statusCode: StatusCodes.Status429TooManyRequests,
            context: context
        );
    }
}
