using _116.Shared.Contracts.Application.CQRS;
using Microsoft.Extensions.Logging;

namespace _116.Shared.Application.Decorators;

/// <summary>
/// Decorator that logs request execution lifecycle and measures performance.
/// Logs start, end, and performance warnings for slow requests (over 3 seconds).
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <param name="handler">The decorated handler.</param>
/// <param name="logger">The logger the lifecycle entries are written to.</param>
/// <param name="timeProvider">The clock used to measure how long the handler took.</param>
public class LoggingDecorator<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> handler,
    ILogger<LoggingDecorator<TRequest, TResponse>> logger,
    TimeProvider timeProvider
) : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    /// <summary>
    /// Handles the incoming request with logging around the execution.
    /// </summary>
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        LogStart();

        long startTimestamp = timeProvider.GetTimestamp();

        TResponse response = await handler.Handle(request, cancellationToken);

        LogPerformanceWarning(timeProvider.GetElapsedTime(startTimestamp));
        LogEnd();

        return response;
    }

    /// <summary>
    /// Logs the start of the request processing. The request payload is intentionally NOT logged:
    /// command records include credentials, OTP codes and refresh tokens, and Serilog would serialize
    /// them in full.
    /// </summary>
    private void LogStart()
    {
        logger.LogInformation("[START] Handling {Request}", typeof(TRequest).Name);
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
        logger.LogDebug("[END] Handled {Request} - Response={Response}", typeof(TRequest).Name, typeof(TResponse).Name);
    }
}
