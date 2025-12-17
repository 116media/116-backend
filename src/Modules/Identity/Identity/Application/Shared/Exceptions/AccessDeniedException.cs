namespace _116.Identity.Application.Shared.Exceptions;

/// <summary>
/// Exception that represents access denied errors (403 Forbidden).
/// </summary>
public class AccessDeniedException : Exception
{
    /// <summary>
    /// Gets additional details about the access denied error, if provided.
    /// </summary>
    public string? Details { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccessDeniedException"/> class with a custom message.
    /// </summary>
    /// <param name="message">The error message that describes the access denied error.</param>
    public AccessDeniedException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccessDeniedException"/> class with a custom message and additional details.
    /// </summary>
    /// <param name="message">The error message that describes the access denied error.</param>
    /// <param name="details">Additional context or information about the access denied error.</param>
    public AccessDeniedException(string message, string details) : base(message)
    {
        Details = details;
    }
}
