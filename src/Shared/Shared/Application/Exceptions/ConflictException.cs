namespace _116.Shared.Application.Exceptions;

/// <summary>
/// Exception for resource conflict scenarios (e.g., duplicate entries, concurrent modifications).
/// </summary>
public class ConflictException : Exception
{
    /// <summary>
    /// Gets additional details about the conflict, if provided.
    /// </summary>
    public string? Details { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class with a custom message.
    /// </summary>
    /// <param name="message">The error message that describes the conflict.</param>
    public ConflictException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class with a custom message and additional details.
    /// </summary>
    /// <param name="message">The error message that describes the conflict.</param>
    /// <param name="details">Additional context or information about the conflict.</param>
    public ConflictException(string message, string details)
        : base(message)
    {
        Details = details;
    }
}
