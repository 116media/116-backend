namespace _116.Identity.Application.Auth.Exceptions;

/// <summary>
/// Exception that represents when a user exceeds the maximum allowed OTP verification attempts.
/// This typically results in a temporary lockout or cooldown period.
/// </summary>
public class OtpAttemptsLimitException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OtpAttemptsLimitException" /> class
    /// with a custom message describing the error.
    /// </summary>
    /// <param name="message">The error message explaining the OTP attempts limit.</param>
    public OtpAttemptsLimitException(string message) : base(message: message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OtpAttemptsLimitException" /> class
    /// with a custom message and additional details.
    /// </summary>
    /// <param name="message">The error message explaining the OTP attempts limit.</param>
    /// <param name="details">Additional context or information about the OTP attempts limit.</param>
    public OtpAttemptsLimitException(string message, string details) : base(message: message)
    {
        Details = details;
    }

    /// <summary>
    /// Gets additional details about the OTP attempts limit, if provided.
    /// </summary>
    public string? Details { get; }
}
