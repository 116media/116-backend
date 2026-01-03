using _116.Identity.Domain.Enums;

namespace _116.Identity.Domain.ValueObjects;

/// <summary>
/// Value object that encapsulates and validates the <see cref="EnumDevicePlatform" /> enum.
/// Provides implicit conversions to and from <see cref="string" /> and <see cref="EnumDevicePlatform" />.
/// </summary>
public record DevicePlatform
{
    /// <summary>
    /// Initializes a new <see cref="DevicePlatform" /> from an <see cref="EnumDevicePlatform" /> enum value.
    /// </summary>
    /// <param name="value">The <see cref="EnumDevicePlatform" /> to wrap.</param>
    /// <exception cref="ArgumentException">Thrown when the provided enum value is not defined.</exception>
    public DevicePlatform(EnumDevicePlatform value)
    {
        if (!Enum.IsDefined(value: value))
        {
            throw new ArgumentException($"Invalid device platform: {value}");
        }

        Value = value;
    }

    /// <summary>
    /// Initializes a new <see cref="DevicePlatform" /> from a string representation.
    /// </summary>
    /// <param name="value">The string to parse into an <see cref="EnumDevicePlatform" />.</param>
    /// <exception cref="ArgumentException">Thrown when the provided string cannot be parsed or is invalid.</exception>
    public DevicePlatform(string value)
    {
        if (!Enum.TryParse(value: value, true, out EnumDevicePlatform parsed) || !Enum.IsDefined(value: parsed))
        {
            throw new ArgumentException($"Invalid device platform: {value}");
        }

        Value = parsed;
    }

    /// <summary>
    /// The validated device platform value.
    /// </summary>
    public EnumDevicePlatform Value { get; init; }

    /// <summary>
    /// Implicit conversion from <see cref="DevicePlatform" /> to <see cref="EnumDevicePlatform" />.
    /// </summary>
    public static implicit operator EnumDevicePlatform(DevicePlatform devicePlatform)
    {
        return devicePlatform.Value;
    }

    /// <summary>
    /// Implicit conversion from <see cref="DevicePlatform" /> to <see cref="string" />.
    /// Returns the string representation of the device platform.
    /// </summary>
    public static implicit operator string(DevicePlatform devicePlatform)
    {
        return devicePlatform.Value.ToString();
    }

    /// <summary>
    /// Implicit conversion from <see cref="EnumDevicePlatform" /> to <see cref="DevicePlatform" />.
    /// </summary>
    public static implicit operator DevicePlatform(EnumDevicePlatform devicePlatform)
    {
        return new DevicePlatform(value: devicePlatform);
    }

    /// <summary>
    /// Implicit conversion from <see cref="string" /> to <see cref="DevicePlatform" />.
    /// </summary>
    public static implicit operator DevicePlatform(string devicePlatform)
    {
        return new DevicePlatform(value: devicePlatform);
    }
}
