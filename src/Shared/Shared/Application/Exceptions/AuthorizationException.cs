namespace _116.Shared.Application.Exceptions;

/// <summary>
/// Exception that represents authorization failures (403 Forbidden).
/// </summary>
public class AuthorizationException : Exception
{
    /// <summary>
    /// Gets additional details about the authorization failure, if provided.
    /// </summary>
    public string? Details { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationException"/> class with a custom message.
    /// </summary>
    /// <param name="message">The error message that describes the authorization failure.</param>
    public AuthorizationException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationException"/> class with a custom message and additional details.
    /// </summary>
    /// <param name="message">The error message that describes the authorization failure.</param>
    /// <param name="details">Additional context or information about the authorization failure.</param>
    public AuthorizationException(string message, string details)
        : base(message)
    {
        Details = details;
    }
}
