namespace _116.Shared.Application.Exceptions;

/// <summary>
/// Exception that represents an invalid format error, typically thrown when a route
/// parameter cannot be parsed as the expected type (e.g. a UUID).
/// </summary>
public class InvalidFormatException : Exception
{
    /// <summary>
    /// Gets additional details about the invalid format error, if provided.
    /// </summary>
    public string? Details { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidFormatException" /> class with a custom message.
    /// </summary>
    /// <param name="message">
    /// The error message that describes the format issue.
    /// </param>
    public InvalidFormatException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidFormatException" /> class with a custom message and additional details.
    /// </summary>
    /// <param name="message">The error message that describes the format issue.</param>
    /// <param name="details">Additional context or information about the invalid format error.</param>
    public InvalidFormatException(string message, string details)
        : base(message)
    {
        Details = details;
    }
}
