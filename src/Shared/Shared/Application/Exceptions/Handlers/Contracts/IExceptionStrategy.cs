using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Shared.Application.Exceptions.Handlers.Contracts;

/// <summary>
/// Defines the contract for handling specific exception types and converting them to ProblemDetails.
/// </summary>
/// <typeparam name="TException">The specific exception type this strategy handles.</typeparam>
public interface IExceptionStrategy<in TException>
    where TException : Exception
{
    /// <summary>
    /// Creates a ProblemDetails response for the given exception.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A ProblemDetails response representing the exception.</returns>
    ProblemDetails CreateProblemDetails(TException exception, HttpContext context);
}

/// <summary>
/// Non-generic marker interface for runtime type discovery.
/// </summary>
public interface IExceptionStrategy
{
    /// <summary>
    /// Gets the exception type this strategy can handle.
    /// </summary>
    Type ExceptionType { get; }

    /// <summary>
    /// Creates a ProblemDetails response for the given exception.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A ProblemDetails response representing the exception.</returns>
    ProblemDetails CreateProblemDetails(Exception exception, HttpContext context);
}
