namespace _116.Shared.Application.Exceptions;

/// <summary>
/// Exception that represents authentication failures (401 Unauthorized).
/// </summary>
public class AuthenticationException : Exception
{
    /// <summary>
    /// Gets additional details about the authentication failure, if provided.
    /// </summary>
    public string? Details { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationException"/> class with a custom message.
    /// </summary>
    /// <param name="message">The error message that describes the authentication failure.</param>
    public AuthenticationException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationException"/> class with a custom message and additional details.
    /// </summary>
    /// <param name="message">The error message that describes the authentication failure.</param>
    /// <param name="details">Additional context or information about the authentication failure.</param>
    public AuthenticationException(string message, string details)
        : base(message)
    {
        Details = details;
    }
}
