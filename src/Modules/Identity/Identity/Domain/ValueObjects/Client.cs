using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Exceptions;
using _116.Identity.Domain.StateMachines;

namespace _116.Identity.Domain.ValueObjects;

/// <summary>
/// Value object that encapsulates and validates the <see cref="EnumClient" /> enum.
/// Provides implicit conversions to and from <see cref="string" /> and <see cref="EnumClient" />.
/// </summary>
public record Client
{
    /// <summary>
    /// Initializes a new <see cref="Client" /> from an <see cref="EnumClient" /> enum value.
    /// </summary>
    /// <param name="value">The <see cref="EnumClient" /> to wrap.</param>
    /// <exception cref="IdentityRuleException">Thrown when the provided enum value is not defined.</exception>
    public Client(EnumClient value)
    {
        if (!Enum.IsDefined(value: value))
        {
            throw new IdentityRuleException(IdentityRuleCodes.InvalidClientPlatform, value.ToString());
        }

        Value = value;
    }

    /// <summary>
    /// Initializes a new <see cref="Client" /> from a string representation.
    /// </summary>
    /// <param name="value">The string to parse into an <see cref="EnumClient" />.</param>
    /// <exception cref="IdentityRuleException">Thrown when the provided string cannot be parsed or is invalid.</exception>
    public Client(string value)
    {
        if (!Enum.TryParse(value: value, true, out EnumClient parsed) || !Enum.IsDefined(value: parsed))
        {
            throw new IdentityRuleException(IdentityRuleCodes.InvalidClientPlatform, value ?? string.Empty);
        }

        Value = parsed;
    }

    /// <summary>
    /// The validated client platform value.
    /// </summary>
    public EnumClient Value { get; init; }

    /// <summary>
    /// Parses a client label into a <see cref="Client" /> without throwing, returning null when
    /// the label is absent or does not match a known platform. Use at untrusted boundaries
    /// (e.g. the <c>Client-App</c> header) where an unrecognized platform is ignored rather
    /// than rejected.
    /// </summary>
    /// <param name="value">The client label, or null when unreported.</param>
    /// <returns>The matched platform, or null when absent or unrecognized.</returns>
    public static Client? TryFrom(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse(value: value, true, out EnumClient parsed) && Enum.IsDefined(value: parsed)
            ? new Client(parsed)
            : null;
    }

    /// <summary>
    /// Implicit conversion from <see cref="Client" /> to <see cref="EnumClient" />.
    /// </summary>
    public static implicit operator EnumClient(Client clientPlatform)
    {
        return clientPlatform.Value;
    }

    /// <summary>
    /// Implicit conversion from <see cref="Client" /> to <see cref="string" />.
    /// Returns the string representation of the client platform.
    /// </summary>
    public static implicit operator string(Client clientPlatform)
    {
        return clientPlatform.Value.ToString();
    }

    /// <summary>
    /// Implicit conversion from <see cref="EnumClient" /> to <see cref="Client" />.
    /// </summary>
    public static implicit operator Client(EnumClient clientPlatform)
    {
        return new Client(value: clientPlatform);
    }

    /// <summary>
    /// Implicit conversion from <see cref="string" /> to <see cref="Client" />.
    /// </summary>
    public static implicit operator Client(string clientPlatform)
    {
        return new Client(value: clientPlatform);
    }
}
