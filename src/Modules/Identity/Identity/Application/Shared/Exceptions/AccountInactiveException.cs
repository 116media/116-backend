namespace _116.Identity.Application.Shared.Exceptions;

/// <summary>
/// Exception that represents a login attempt for an account
/// that has been deactivated or disabled and cannot currently log in.
/// </summary>
public class AccountInactiveException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountInactiveException" /> class
    /// with a custom message describing the error.
    /// </summary>
    /// <param name="message">The error message explaining the account inactivity.</param>
    public AccountInactiveException(string message) : base(message: message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountInactiveException" /> class
    /// with a custom message and additional details.
    /// </summary>
    /// <param name="message">The error message explaining the account inactivity.</param>
    /// <param name="details">Additional context or information about why the account is inactive.</param>
    public AccountInactiveException(string message, string details) : base(message: message)
    {
        Details = details;
    }

    /// <summary>
    /// Gets additional details about why the account is inactive, if provided.
    /// </summary>
    public string? Details { get; }
}
