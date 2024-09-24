namespace _116.Auth.Domain.ValueObjects;

/// <summary>
/// Value object that encapsulates and validates the <see cref="OtpPurpose"/> enum.
/// Provides implicit conversions to and from <see cref="string"/> and <see cref="OtpPurpose"/>.
/// </summary>
public record OtpPurpose
{
    /// <summary>
    /// The validated OTP purpose value.
    /// </summary>
    public Enums.OtpPurpose Value { get; init; }

    /// <summary>
    /// Initializes a new <see cref="OtpPurpose"/> from an <see cref="Enums.OtpPurpose"/> enum value.
    /// </summary>
    /// <param name="value">The <see cref="Enums.OtpPurpose"/> to wrap.</param>
    /// <exception cref="ArgumentException">Thrown when the provided enum value is not defined.</exception>
    public OtpPurpose(Enums.OtpPurpose value)
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentException($"Invalid OTP purpose: {value}");
        }

        Value = value;
    }

    /// <summary>
    /// Initializes a new <see cref="OtpPurpose"/> from a string representation.
    /// </summary>
    /// <param name="value">The string to parse into an <see cref="Enums.OtpPurpose"/>.</param>
    /// <exception cref="ArgumentException">Thrown when the provided string cannot be parsed or is invalid.</exception>
    public OtpPurpose(string value)
    {
        if (!Enum.TryParse(value, true, out Enums.OtpPurpose parsed) || !Enum.IsDefined(parsed))
        {
            throw new ArgumentException($"Invalid OTP purpose: {value}");
        }

        Value = parsed;
    }

    /// <summary>
    /// Implicit conversion from <see cref="OtpPurpose"/> to <see cref="Enums.OtpPurpose"/>.
    /// </summary>
    public static implicit operator Enums.OtpPurpose(OtpPurpose otpPurpose) => otpPurpose.Value;

    /// <summary>
    /// Implicit conversion from <see cref="OtpPurpose"/> to <see cref="string"/>.
    /// Returns the string representation of the OTP purpose.
    /// </summary>
    public static implicit operator string(OtpPurpose otpPurpose) => otpPurpose.Value.ToString();

    /// <summary>
    /// Implicit conversion from <see cref="Enums.OtpPurpose"/> to <see cref="OtpPurpose"/>.
    /// </summary>
    public static implicit operator OtpPurpose(Enums.OtpPurpose otpPurpose) => new(otpPurpose);

    /// <summary>
    /// Implicit conversion from <see cref="string"/> to <see cref="OtpPurpose"/>.
    /// </summary>
    public static implicit operator OtpPurpose(string otpPurpose) => new(otpPurpose);
}
