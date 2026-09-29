using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Exceptions;
using _116.Identity.Domain.StateMachines;

namespace _116.Identity.Domain.ValueObjects;

/// <summary>
/// Value object that encapsulates and validates the <see cref="OtpPurpose" /> enum.
/// Provides implicit conversions to and from <see cref="string" /> and <see cref="OtpPurpose" />.
/// </summary>
public record OtpPurpose
{
    /// <summary>
    /// Initializes a new <see cref="OtpPurpose" /> from an <see cref="EnumOtpPurpose" /> enum value.
    /// </summary>
    /// <param name="value">The <see cref="EnumOtpPurpose" /> to wrap.</param>
    /// <exception cref="IdentityRuleException">Thrown when the provided enum value is not defined.</exception>
    public OtpPurpose(EnumOtpPurpose value)
    {
        if (!Enum.IsDefined(value: value))
        {
            throw new IdentityRuleException(IdentityRuleCodes.InvalidOtpPurpose, value.ToString());
        }

        Value = value;
    }

    /// <summary>
    /// Initializes a new <see cref="OtpPurpose" /> from a string representation.
    /// </summary>
    /// <param name="value">The string to parse into an <see cref="EnumOtpPurpose" />.</param>
    /// <exception cref="IdentityRuleException">Thrown when the provided string cannot be parsed or is invalid.</exception>
    public OtpPurpose(string value)
    {
        if (!Enum.TryParse(value: value, true, out EnumOtpPurpose parsed) || !Enum.IsDefined(value: parsed))
        {
            throw new IdentityRuleException(IdentityRuleCodes.InvalidOtpPurpose, value ?? string.Empty);
        }

        Value = parsed;
    }

    /// <summary>
    /// The validated OTP purpose value.
    /// </summary>
    public EnumOtpPurpose Value { get; init; }

    /// <summary>
    /// Implicit conversion from <see cref="OtpPurpose" /> to <see cref="EnumOtpPurpose" />.
    /// </summary>
    public static implicit operator EnumOtpPurpose(OtpPurpose otpPurpose)
    {
        return otpPurpose.Value;
    }

    /// <summary>
    /// Implicit conversion from <see cref="OtpPurpose" /> to <see cref="string" />.
    /// Returns the string representation of the OTP purpose.
    /// </summary>
    public static implicit operator string(OtpPurpose otpPurpose)
    {
        return otpPurpose.Value.ToString();
    }

    /// <summary>
    /// Implicit conversion from <see cref="EnumOtpPurpose" /> to <see cref="OtpPurpose" />.
    /// </summary>
    public static implicit operator OtpPurpose(EnumOtpPurpose otpPurpose)
    {
        return new OtpPurpose(value: otpPurpose);
    }

    /// <summary>
    /// Implicit conversion from <see cref="string" /> to <see cref="OtpPurpose" />.
    /// </summary>
    public static implicit operator OtpPurpose(string otpPurpose)
    {
        return new OtpPurpose(value: otpPurpose);
    }
}
