using System.Net.Mail;

namespace _116.Auth.Domain.ValueObjects;

/// <summary>
/// Represents a validated email address as a value object.
/// </summary>
/// <remarks>
/// The <see cref="Email"/> record ensures that the provided string is non-empty
/// and conforms to a valid email format. It also normalizes the value
/// to lowercase for consistency.
/// </remarks>
public record Email
{
    /// <summary>
    /// Gets the normalized email address value in lowercase.
    /// </summary>
    public string Value { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Email"/> record with validation.
    /// </summary>
    /// <param name="value">The email address strings to validate and store.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the <paramref name="value"/> is null, empty, whitespace,
    /// or not a valid email format.
    /// </exception>
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email cannot be empty", nameof(value));
        }

        if (!IsValidEmail(value))
        {
            throw new ArgumentException("Invalid email format", nameof(value));
        }

        Value = value.ToLowerInvariant();
    }

    /// <summary>
    /// Determines whether the specified string is a valid email address.
    /// </summary>
    /// <param name="email">The email-string to validate.</param>
    /// <returns><c>true</c> if the string is a valid email address; otherwise, <c>false</c>.</returns>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Implicitly converts an <see cref="Email"/> to a <see cref="string"/>.
    /// </summary>
    /// <param name="email">The <see cref="Email"/> instance.</param>
    public static implicit operator string(Email email) => email.Value;

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="Email"/> instance.
    /// </summary>
    /// <param name="email">The email string to convert.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the <paramref name="email"/> is not valid.
    /// </exception>
    public static implicit operator Email(string email) => new(email);
}
