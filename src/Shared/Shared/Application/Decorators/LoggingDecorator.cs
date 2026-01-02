using System.Diagnostics;

using _116.Shared.Contracts.Application.CQRS;

using Microsoft.Extensions.Logging;

namespace _116.Shared.Application.Decorators;

/// <summary>
/// Decorator that logs request execution lifecycle and measures performance.
/// Logs start, end, and performance warnings for slow requests (over 3 seconds).
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public class LoggingDecorator<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> handler,
    ILogger<LoggingDecorator<TRequest, TResponse>> logger
) : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    /// <summary>
    /// Handles the incoming request with logging around the execution.
    /// </summary>
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        LogStart(request);

        var stopwatch = Stopwatch.StartNew();

        TResponse response = await handler.Handle(request, cancellationToken);

        stopwatch.Stop();

        LogPerformanceWarning(stopwatch.Elapsed);
        LogEnd();

        return response;
    }

    /// <summary>
    /// Logs the start of the request processing.
    /// </summary>
    private void LogStart(TRequest request)
    {
        logger.LogInformation(
            "[START] Handling {Request} - RequestData={RequestData}",
            typeof(TRequest).Name,
            request
        );
    }

    /// <summary>
    /// Logs a performance warning if the elapsed time exceeds the threshold.
    /// </summary>
    private void LogPerformanceWarning(TimeSpan elapsed)
    {
        // if the request takes more than 3 seconds, then log the warnings
        if (elapsed.TotalSeconds > 3)
        {
            logger.LogWarning(
                "[PERFORMANCE] Request {Request} took {ElapsedSeconds:N2} seconds.",
                typeof(TRequest).Name,
                elapsed.TotalSeconds
            );
        }
    }

    /// <summary>
    /// Logs the end of request processing along with the response type.
    /// </summary>
    private void LogEnd()
    {
        logger.LogInformation(
            "[END] Handled {Request} - Response={Response}",
            typeof(TRequest).Name,
            typeof(TResponse).Name
        );
    }
}
