namespace _116.Shared.Application.Exceptions;

/// <summary>
/// Exception that represents a bad request, typically caused by invalid input or parameters.
/// </summary>
public class BadRequestException : Exception
{
    /// <summary>
    /// Gets additional details about the bad request, if provided.
    /// </summary>
    public string? Details { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadRequestException"/> class with a custom message.
    /// </summary>
    /// <param name="message">The error message that describes the issue.</param>
    public BadRequestException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadRequestException"/> class with a custom message and additional details.
    /// </summary>
    /// <param name="message">The error message that describes the issue.</param>
    /// <param name="details">Additional context or information about the bad request.</param>
    public BadRequestException(string message, string details)
        : base(message)
    {
        Details = details;
    }
}
