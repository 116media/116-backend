using _116.Content.Domain.Enums;

namespace _116.Content.Domain.ValueObjects;

/// <summary>
/// Value object that encapsulates and validates the <see cref="EnumShareChannel" /> enum.
/// Provides implicit conversions to and from <see cref="string" /> and <see cref="EnumShareChannel" />.
/// </summary>
public record ShareChannel
{
    /// <summary>
    /// Initializes a new <see cref="ShareChannel" /> from an <see cref="EnumShareChannel" /> value.
    /// </summary>
    /// <param name="value">The <see cref="EnumShareChannel" /> to wrap.</param>
    /// <exception cref="ArgumentException">Thrown when the provided enum value is not defined.</exception>
    public ShareChannel(EnumShareChannel value)
    {
        if (!Enum.IsDefined(value: value))
        {
            throw new ArgumentException($"Invalid share channel: {value}");
        }

        Value = value;
    }

    /// <summary>
    /// Initializes a new <see cref="ShareChannel" /> from a string representation.
    /// </summary>
    /// <param name="value">The string to parse into an <see cref="EnumShareChannel" />.</param>
    /// <exception cref="ArgumentException">Thrown when the provided string cannot be parsed or is invalid.</exception>
    public ShareChannel(string value)
    {
        if (!Enum.TryParse(value: value, true, out EnumShareChannel parsed) || !Enum.IsDefined(value: parsed))
        {
            throw new ArgumentException($"Invalid share channel: {value}");
        }

        Value = parsed;
    }

    /// <summary>
    /// The validated share channel value.
    /// </summary>
    public EnumShareChannel Value { get; init; }

    /// <summary>
    /// Parses a client label into a <see cref="ShareChannel" /> without throwing, returning
    /// null when the label is absent or does not match a known channel. Use at untrusted
    /// boundaries (e.g. request bodies) where an unrecognized channel is ignored rather than
    /// rejected.
    /// </summary>
    /// <param name="value">The client label, or null when unreported.</param>
    /// <returns>The matched channel, or null when absent or unrecognized.</returns>
    public static ShareChannel? TryFrom(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse(value: value, true, out EnumShareChannel parsed) && Enum.IsDefined(value: parsed)
            ? new ShareChannel(parsed)
            : null;
    }

    /// <summary>
    /// Implicit conversion from <see cref="ShareChannel" /> to <see cref="EnumShareChannel" />.
    /// </summary>
    public static implicit operator EnumShareChannel(ShareChannel shareChannel)
    {
        return shareChannel.Value;
    }

    /// <summary>
    /// Implicit conversion from <see cref="ShareChannel" /> to <see cref="string" />.
    /// Returns the string representation of the share channel.
    /// </summary>
    public static implicit operator string(ShareChannel shareChannel)
    {
        return shareChannel.Value.ToString();
    }

    /// <summary>
    /// Implicit conversion from <see cref="EnumShareChannel" /> to <see cref="ShareChannel" />.
    /// </summary>
    public static implicit operator ShareChannel(EnumShareChannel value)
    {
        return new ShareChannel(value: value);
    }

    /// <summary>
    /// Implicit conversion from <see cref="string" /> to <see cref="ShareChannel" />.
    /// </summary>
    public static implicit operator ShareChannel(string value)
    {
        return new ShareChannel(value: value);
    }
}
