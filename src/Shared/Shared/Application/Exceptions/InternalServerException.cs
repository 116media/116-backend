namespace _116.Shared.Application.Exceptions;

/// <summary>
/// Exception that represents an internal server error, typically indicating an unexpected failure during execution.
/// </summary>
public class InternalServerException : Exception
{
    /// <summary>
    /// Gets additional details about the internal server error, if provided.
    /// </summary>
    public string? Details { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InternalServerException"/> class with a custom message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public InternalServerException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InternalServerException"/> class with a custom message and additional details.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="details">Additional context or details related to the error.</param>
    public InternalServerException(string message, string details)
        : base(message)
    {
        Details = details;
    }
}
