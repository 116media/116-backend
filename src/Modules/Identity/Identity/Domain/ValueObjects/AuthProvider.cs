using _116.Identity.Domain.Enums;

namespace _116.Identity.Domain.ValueObjects;

/// <summary>
/// Value object that encapsulates and validates the <see cref="AuthProvider"/> enum.
/// Provides implicit conversions to and from <see cref="string"/> and <see cref="AuthProvider"/>.
/// </summary>
public record AuthProvider
{
    /// <summary>
    /// The validated authentication provider value.
    /// </summary>
    public EnumAuthProvider Value { get; init; }
    /// <summary>
    /// Initializes a new <see cref="AuthProvider"/> from an <see cref="AuthProvider"/> enum value.
    /// </summary>
    /// <param name="value">The <see cref="AuthProvider"/> to wrap.</param>
    /// <exception cref="ArgumentException">Thrown when the provided enum value is not defined.</exception>
    public AuthProvider(EnumAuthProvider value)
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentException($"Invalid auth provider: {value}");
        }
        Value = value;
    }
    /// <summary>
    /// Initializes a new <see cref="AuthProvider"/> from a string representation.
    /// </summary>
    /// <param name="value">The string to parse into an <see cref="AuthProvider"/>.</param>
    /// <exception cref="ArgumentException">Thrown when the provided string cannot be parsed or is invalid.</exception>
    public AuthProvider(string value)
    {
        if (!Enum.TryParse(value, true, out EnumAuthProvider parsed) || !Enum.IsDefined(parsed))
        {
            throw new ArgumentException($"Invalid auth provider: {value}");
        }
        Value = parsed;
    }
    /// <summary>
    /// Implicit conversion from <see cref="AuthProvider"/> to <see cref="AuthProvider"/>.
    /// </summary>
    public static implicit operator EnumAuthProvider(AuthProvider provider) => provider.Value;
    /// <summary>
    /// Implicit conversion from <see cref="AuthProvider"/> to <see cref="string"/>.
    /// Returns the string representation of the provider.
    /// </summary>
    public static implicit operator string(AuthProvider provider) => provider.Value.ToString();
    /// <summary>
    /// Implicit conversion from <see cref="AuthProvider"/> to <see cref="AuthProvider"/>.
    /// </summary>
    public static implicit operator AuthProvider(EnumAuthProvider provider) => new(provider);
    /// <summary>
    /// Implicit conversion from <see cref="string"/> to <see cref="AuthProvider"/>.
    /// </summary>
    public static implicit operator AuthProvider(string provider) => new(provider);
}
