namespace _116.Shared.Application.Exceptions;

/// <summary>
/// Exception that represents an upstream service failure (HTTP 502 Bad Gateway),
/// typically when a dependent external service returns an invalid or unexpected response.
/// </summary>
public class BadGatewayException : Exception
{
    /// <summary>
    /// Gets additional details about the upstream failure, if provided.
    /// </summary>
    public string? Details { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadGatewayException"/> class with a custom message.
    /// </summary>
    /// <param name="message">The error message describing the upstream failure.</param>
    public BadGatewayException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadGatewayException"/> class with a custom message and additional details.
    /// </summary>
    /// <param name="message">The error message describing the upstream failure.</param>
    /// <param name="details">Additional context or information about the upstream service error.</param>
    public BadGatewayException(string message, string details)
        : base(message)
    {
        Details = details;
    }
}
