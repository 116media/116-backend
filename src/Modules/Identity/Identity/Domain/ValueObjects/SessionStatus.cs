using _116.Identity.Domain.Enums;

namespace _116.Identity.Domain.ValueObjects;

/// <summary>
/// Value object that encapsulates and validates the <see cref="EnumSessionStatus" /> enum.
/// Provides implicit conversions to and from <see cref="string" /> and <see cref="EnumSessionStatus" />.
/// </summary>
public record SessionStatus
{
    /// <summary>
    /// Initializes a new <see cref="SessionStatus" /> from an <see cref="EnumSessionStatus" /> enum value.
    /// </summary>
    /// <param name="value">The <see cref="EnumSessionStatus" /> to wrap.</param>
    /// <exception cref="ArgumentException">Thrown when the provided enum value is not defined.</exception>
    public SessionStatus(EnumSessionStatus value)
    {
        if (!Enum.IsDefined(value: value))
        {
            throw new ArgumentException($"Invalid session status: {value}");
        }

        Value = value;
    }

    /// <summary>
    /// Initializes a new <see cref="SessionStatus" /> from a string representation.
    /// </summary>
    /// <param name="value">The string to parse into an <see cref="EnumSessionStatus" />.</param>
    /// <exception cref="ArgumentException">Thrown when the provided string cannot be parsed or is invalid.</exception>
    public SessionStatus(string value)
    {
        if (!Enum.TryParse(value: value, true, out EnumSessionStatus parsed) || !Enum.IsDefined(value: parsed))
        {
            throw new ArgumentException($"Invalid session status: {value}");
        }

        Value = parsed;
    }

    /// <summary>
    /// The validated session status value.
    /// </summary>
    public EnumSessionStatus Value { get; init; }

    /// <summary>
    /// Implicit conversion from <see cref="SessionStatus" /> to <see cref="EnumSessionStatus" />.
    /// </summary>
    public static implicit operator EnumSessionStatus(SessionStatus sessionStatus)
    {
        return sessionStatus.Value;
    }

    /// <summary>
    /// Implicit conversion from <see cref="SessionStatus" /> to <see cref="string" />.
    /// Returns the string representation of the session status.
    /// </summary>
    public static implicit operator string(SessionStatus sessionStatus)
    {
        return sessionStatus.Value.ToString();
    }

    /// <summary>
    /// Implicit conversion from <see cref="EnumSessionStatus" /> to <see cref="SessionStatus" />.
    /// </summary>
    public static implicit operator SessionStatus(EnumSessionStatus sessionStatus)
    {
        return new SessionStatus(value: sessionStatus);
    }

    /// <summary>
    /// Implicit conversion from <see cref="string" /> to <see cref="SessionStatus" />.
    /// </summary>
    public static implicit operator SessionStatus(string sessionStatus)
    {
        return new SessionStatus(value: sessionStatus);
    }
}
