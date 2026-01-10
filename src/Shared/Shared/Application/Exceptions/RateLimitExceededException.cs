namespace _116.Shared.Application.Exceptions;

/// <summary>
/// Exception that represents a rate limit violation.
/// Thrown when a client exceeds the configured rate limit for an endpoint.
/// </summary>
public class RateLimitExceededException : Exception
{
    /// <summary>
    /// Gets the time period after which the client can retry the request.
    /// </summary>
    public TimeSpan RetryAfter { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitExceededException"/> class.
    /// </summary>
    /// <param name="retryAfter">The time period after which the client can retry.</param>
    public RateLimitExceededException(TimeSpan retryAfter)
        : base($"Rate limit exceeded. Please try again in {(int)retryAfter.TotalSeconds} seconds.")
    {
        RetryAfter = retryAfter;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitExceededException"/> class with a custom message.
    /// </summary>
    /// <param name="message">The error message that describes the rate limit violation.</param>
    /// <param name="retryAfter">The time period after which the client can retry.</param>
    public RateLimitExceededException(string message, TimeSpan retryAfter)
        : base(message)
    {
        RetryAfter = retryAfter;
    }
}
