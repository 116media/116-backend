using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace _116.Shared.Application.Exceptions.Handlers;

/// <summary>
/// Enterprise-grade exception handler using the Strategy pattern for maximum maintainability.
/// </summary>
/// <remarks>
/// This handler uses a pluggable strategy system that automatically discovers and maps exception types
/// to their corresponding handling strategies. Adding new exception types requires only creating a new
/// strategy class - no modifications to this handler are needed.
///
/// Features:
/// - Strategy pattern eliminates switch statements and promotes Open/Closed principle
/// - Automatic strategy discovery via dependency injection
/// - Inheritance hierarchy traversal for exception type resolution
/// - Thread-safe strategy caching for optimal performance
/// - Comprehensive logging with structured data
///
/// To enable this handler, register it in the service container:
/// <code>
/// builder.Services.AddGlobalExceptionHandler();
/// app.UseGlobalExceptionHandler();
/// </code>
/// </remarks>
public sealed class ExceptionHandler(ILogger<ExceptionHandler> logger, ExceptionStrategyRegistry strategyRegistry)
    : IExceptionHandler
{
    /// <summary>
    /// Attempts to handle an unhandled exception using the appropriate strategy.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="exception">The unhandled exception.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Always returns true, indicating the exception was handled.</returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        // Log the exception with structured data for observability
        LogException(exception, context);

        // Use strategy pattern to create appropriate response
        ProblemDetails problemDetails = strategyRegistry.CreateProblemDetails(exception, context);

        // Enrich with request metadata
        EnrichProblemDetails(problemDetails, context);

        // Send response
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);

        return true;
    }

    /// <summary>
    /// Logs exception details with structured data for observability and monitoring.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="context">The HTTP context for request details.</param>
    private void LogException(Exception exception, HttpContext context)
    {
        LogLevel logLevel = DetermineLogLevel(exception);

        logger.Log(
            logLevel,
            exception,
            "Exception {ExceptionType} occurred on {RequestMethod} {RequestPath} at {Timestamp}. "
                + "TraceId: {TraceId}, User: {UserId}",
            exception.GetType().Name,
            context.Request.Method,
            context.Request.Path,
            DateTime.UtcNow,
            context.TraceIdentifier,
            context.User.Identity?.Name ?? "Anonymous"
        );
    }

    /// <summary>
    /// Determines the appropriate log level based on exception type and severity.
    /// </summary>
    /// <param name="exception">The exception to evaluate.</param>
    /// <returns>The appropriate log level for this exception.</returns>
    private static LogLevel DetermineLogLevel(Exception exception)
    {
        return exception switch
        {
            BadRequestException => LogLevel.Warning,
            NotFoundException => LogLevel.Warning,
            ConflictException => LogLevel.Warning,
            AuthenticationException => LogLevel.Warning,
            AuthorizationException => LogLevel.Warning,
            _ => LogLevel.Error,
        };
    }

    /// <summary>
    /// Enriches the ProblemDetails with additional context and metadata.
    /// </summary>
    /// <param name="problemDetails">The problem details to enrich.</param>
    /// <param name="context">The HTTP context containing request information.</param>
    private static void EnrichProblemDetails(ProblemDetails problemDetails, HttpContext context)
    {
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;
    }
}
