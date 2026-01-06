using _116.Shared.Application.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Shared.Application.Exceptions.Handlers.Strategies;

/// <summary>
/// Default strategy for handling unregistered exception types.
/// This strategy handles the base Exception type and serves as a fallback.
/// </summary>
public sealed class DefaultExceptionHandler : IExceptionStrategy
{
    /// <inheritdoc />
    public Type ExceptionType => typeof(Exception);

    /// <inheritdoc />
    public ProblemDetails CreateProblemDetails(Exception exception, HttpContext context)
    {
        return new ProblemDetails
        {
            Title = exception.GetType().Name,
            Detail = exception.Message,
            Status = StatusCodes.Status500InternalServerError,
            Instance = context.Request.Path,
        };
    }
}
