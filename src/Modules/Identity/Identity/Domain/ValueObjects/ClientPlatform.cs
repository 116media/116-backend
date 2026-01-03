using _116.Identity.Domain.Enums;

namespace _116.Identity.Domain.ValueObjects;

/// <summary>
/// Value object that encapsulates and validates the <see cref="EnumClientPlatform" /> enum.
/// Provides implicit conversions to and from <see cref="string" /> and <see cref="EnumClientPlatform" />.
/// </summary>
public record ClientPlatform
{
    /// <summary>
    /// Initializes a new <see cref="ClientPlatform" /> from an <see cref="EnumClientPlatform" /> enum value.
    /// </summary>
    /// <param name="value">The <see cref="EnumClientPlatform" /> to wrap.</param>
    /// <exception cref="ArgumentException">Thrown when the provided enum value is not defined.</exception>
    public ClientPlatform(EnumClientPlatform value)
    {
        if (!Enum.IsDefined(value: value))
        {
            throw new ArgumentException($"Invalid client platform: {value}");
        }

        Value = value;
    }

    /// <summary>
    /// Initializes a new <see cref="ClientPlatform" /> from a string representation.
    /// </summary>
    /// <param name="value">The string to parse into an <see cref="EnumClientPlatform" />.</param>
    /// <exception cref="ArgumentException">Thrown when the provided string cannot be parsed or is invalid.</exception>
    public ClientPlatform(string value)
    {
        if (!Enum.TryParse(value: value, true, out EnumClientPlatform parsed) || !Enum.IsDefined(value: parsed))
        {
            throw new ArgumentException($"Invalid client platform: {value}");
        }

        Value = parsed;
    }

    /// <summary>
    /// The validated client platform value.
    /// </summary>
    public EnumClientPlatform Value { get; init; }

    /// <summary>
    /// Implicit conversion from <see cref="ClientPlatform" /> to <see cref="EnumClientPlatform" />.
    /// </summary>
    public static implicit operator EnumClientPlatform(ClientPlatform clientPlatform)
    {
        return clientPlatform.Value;
    }

    /// <summary>
    /// Implicit conversion from <see cref="ClientPlatform" /> to <see cref="string" />.
    /// Returns the string representation of the client platform.
    /// </summary>
    public static implicit operator string(ClientPlatform clientPlatform)
    {
        return clientPlatform.Value.ToString();
    }

    /// <summary>
    /// Implicit conversion from <see cref="EnumClientPlatform" /> to <see cref="ClientPlatform" />.
    /// </summary>
    public static implicit operator ClientPlatform(EnumClientPlatform clientPlatform)
    {
        return new ClientPlatform(value: clientPlatform);
    }

    /// <summary>
    /// Implicit conversion from <see cref="string" /> to <see cref="ClientPlatform" />.
    /// </summary>
    public static implicit operator ClientPlatform(string clientPlatform)
    {
        return new ClientPlatform(value: clientPlatform);
    }
}
