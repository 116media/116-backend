namespace _116.Identity.Application.Shared.Exceptions;

/// <summary>
/// Exception that represents when an OTP code has expired and is no longer valid for verification.
/// </summary>
public class OtpExpirationException : Exception
{
    /// <summary>
    /// Gets additional details about the OTP expiration, if provided.
    /// </summary>
    public string? Details { get; }
    /// <summary>
    /// Initializes a new instance of the <see cref="OtpExpirationException"/> class
    /// with a custom message describing the error.
    /// </summary>
    /// <param name="message">The error message explaining the OTP expiration.</param>
    public OtpExpirationException(string message) : base(message) { }
    /// <summary>
    /// Initializes a new instance of the <see cref="OtpExpirationException"/> class
    /// with a custom message and additional details.
    /// </summary>
    /// <param name="message">The error message explaining the OTP expiration.</param>
    /// <param name="details">Additional context or information about the OTP expiration.</param>
    public OtpExpirationException(string message, string details) : base(message)
    {
        Details = details;
    }
}
