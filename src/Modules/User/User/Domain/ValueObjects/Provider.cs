using _116.User.Domain.Enums;

namespace _116.User.Domain.ValueObjects;

/// <summary>
/// Value object that encapsulates and validates the <see cref="AuthProvider"/> enum.
/// Provides implicit conversions to and from <see cref="string"/> and <see cref="AuthProvider"/>.
/// </summary>
public record Provider
{
    /// <summary>
    /// The validated authentication provider value.
    /// </summary>
    public AuthProvider Value { get; init; }

    /// <summary>
    /// Initializes a new <see cref="Provider"/> from an <see cref="AuthProvider"/> enum value.
    /// </summary>
    /// <param name="value">The <see cref="AuthProvider"/> to wrap.</param>
    /// <exception cref="ArgumentException">Thrown when the provided enum value is not defined.</exception>
    public Provider(AuthProvider value)
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentException($"Invalid auth provider: {value}");
        }

        Value = value;
    }

    /// <summary>
    /// Initializes a new <see cref="Provider"/> from a string representation.
    /// </summary>
    /// <param name="value">The string to parse into an <see cref="AuthProvider"/>.</param>
    /// <exception cref="ArgumentException">Thrown when the provided string cannot be parsed or is invalid.</exception>
    public Provider(string value)
    {
        if (!Enum.TryParse(value, true, out AuthProvider parsed) || !Enum.IsDefined(parsed))
        {
            throw new ArgumentException($"Invalid auth provider: {value}");
        }

        Value = parsed;
    }

    /// <summary>
    /// Implicit conversion from <see cref="Provider"/> to <see cref="AuthProvider"/>.
    /// </summary>
    public static implicit operator AuthProvider(Provider provider) => provider.Value;

    /// <summary>
    /// Implicit conversion from <see cref="Provider"/> to <see cref="string"/>.
    /// Returns the string representation of the provider.
    /// </summary>
    public static implicit operator string(Provider provider) => provider.Value.ToString();

    /// <summary>
    /// Implicit conversion from <see cref="AuthProvider"/> to <see cref="Provider"/>.
    /// </summary>
    public static implicit operator Provider(AuthProvider provider) => new(provider);

    /// <summary>
    /// Implicit conversion from <see cref="string"/> to <see cref="Provider"/>.
    /// </summary>
    public static implicit operator Provider(string provider) => new(provider);
}
