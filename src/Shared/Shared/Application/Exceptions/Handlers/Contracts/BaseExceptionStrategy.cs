using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Shared.Application.Exceptions.Handlers.Contracts;

/// <summary>
/// Abstract base class for exception strategies providing common functionality.
/// </summary>
/// <typeparam name="TException">The specific exception type this strategy handles.</typeparam>
public abstract class BaseExceptionStrategy<TException>
    : IExceptionStrategy<TException>, IExceptionStrategy where TException : Exception
{
    /// <inheritdoc />
    public Type ExceptionType => typeof(TException);

    /// <inheritdoc />
    public abstract ProblemDetails CreateProblemDetails(TException exception, HttpContext context);

    /// <inheritdoc />
    public ProblemDetails CreateProblemDetails(Exception exception, HttpContext context)
    {
        return CreateProblemDetails((TException)exception, context);
    }

    /// <summary>
    /// Creates a standardized ProblemDetails object.
    /// </summary>
    /// <param name="title">The error title.</param>
    /// <param name="detail">The error detail message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A configured ProblemDetails object.</returns>
    protected static ProblemDetails CreateStandardProblemDetails(
        string title,
        string detail,
        int statusCode,
        HttpContext context
    )
    {
        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = context.TraceIdentifier,
                ["timestamp"] = DateTime.UtcNow
            }
        };

        return problemDetails;
    }
}
