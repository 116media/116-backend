using _116.Shared.Application.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Shared.Application.Exceptions.Handlers.Strategies;

/// <summary>
/// Strategy for handling <see cref="BadHttpRequestException" /> instances, so a request-body
/// size violation surfaces as 413 instead of the generic 500. The exception carries the status
/// it was raised with, which is preserved for its other shapes (malformed requests stay 400).
/// </summary>
public sealed class RequestBodyLimitExceptionStrategy : BaseExceptionStrategy<BadHttpRequestException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(BadHttpRequestException exception, HttpContext context)
    {
        string title =
            exception.StatusCode == StatusCodes.Status413PayloadTooLarge ? "PayloadTooLarge" : "BadHttpRequest";

        return CreateStandardProblemDetails(
            title: title,
            detail: exception.Message,
            statusCode: exception.StatusCode,
            context: context
        );
    }
}
