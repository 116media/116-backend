namespace _116.Identity.Application.Shared.Exceptions;

/// <summary>
/// Exception that represents a login attempt for an account
/// that has not yet been verified (e.g., email or phone confirmation pending).
/// </summary>
public class AccountNotVerifiedException : Exception
{
    /// <summary>
    /// Gets additional details about why the account is not verified, if provided.
    /// </summary>
    public string? Details { get; }
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountNotVerifiedException"/> class
    /// with a custom message describing the error.
    /// </summary>
    /// <param name="message">The error message that explains the verification issue.</param>
    public AccountNotVerifiedException(string message) : base(message) { }
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountNotVerifiedException"/> class
    /// with a custom message and additional details.
    /// </summary>
    /// <param name="message">The error message that explains the verification issue.</param>
    /// <param name="details">Additional context or information about the account’s unverified status.</param>
    public AccountNotVerifiedException(string message, string details) : base(message)
    {
        Details = details;
    }
}
