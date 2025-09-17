namespace _116.User.Application.Shared.Exceptions;

/// <summary>
/// Exception that represents an attempt to access a resource
/// when the user is not currently logged in or their session has expired.
/// </summary>
public class UserNotLoggedInException : Exception
{
    /// <summary>
    /// Gets additional details about the login state, if provided.
    /// </summary>
    public string? Details { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserNotLoggedInException"/> class
    /// with a custom message describing the error.
    /// </summary>
    /// <param name="message">The error message explaining the login requirement.</param>
    public UserNotLoggedInException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserNotLoggedInException"/> class
    /// with a custom message and additional details.
    /// </summary>
    /// <param name="message">The error message explaining the login requirement.</param>
    /// <param name="details">Additional context or information about the login state.</param>
    public UserNotLoggedInException(string message, string details) : base(message)
    {
        Details = details;
    }
}
