using _116.Identity.Domain.Enums;

namespace _116.Identity.Domain.ValueObjects;

/// <summary>
/// Value object that encapsulates and validates the <see cref="SessionExportFormat" /> enum.
/// Provides implicit conversions to and from <see cref="string" /> and <see cref="SessionExportFormat" />.
/// </summary>
public record ExportFormat
{
    /// <summary>
    /// Initializes a new <see cref="ExportFormat" /> from a <see cref="SessionExportFormat" /> enum value.
    /// </summary>
    /// <param name="value">The <see cref="SessionExportFormat" /> to wrap.</param>
    /// <exception cref="ArgumentException">Thrown when the provided enum value is not defined.</exception>
    public ExportFormat(SessionExportFormat value)
    {
        if (!Enum.IsDefined(value: value))
        {
            throw new ArgumentException($"Invalid export format: {value}");
        }

        Value = value;
    }

    /// <summary>
    /// Initializes a new <see cref="ExportFormat" /> from a string representation.
    /// </summary>
    /// <param name="value">The string to parse into a <see cref="SessionExportFormat" />.</param>
    /// <exception cref="ArgumentException">Thrown when the provided string cannot be parsed or is invalid.</exception>
    public ExportFormat(string value)
    {
        if (!Enum.TryParse(value: value, true, out SessionExportFormat parsed) || !Enum.IsDefined(value: parsed))
        {
            throw new ArgumentException($"Invalid export format: {value}");
        }

        Value = parsed;
    }

    /// <summary>
    /// The validated export format value.
    /// </summary>
    public SessionExportFormat Value { get; init; }

    /// <summary>
    /// Implicit conversion from <see cref="ExportFormat" /> to <see cref="SessionExportFormat" />.
    /// </summary>
    public static implicit operator SessionExportFormat(ExportFormat exportFormat)
    {
        return exportFormat.Value;
    }

    /// <summary>
    /// Implicit conversion from <see cref="ExportFormat" /> to <see cref="string" />.
    /// Returns the string representation of the export format.
    /// </summary>
    public static implicit operator string(ExportFormat exportFormat)
    {
        return exportFormat.Value.ToString();
    }

    /// <summary>
    /// Implicit conversion from <see cref="SessionExportFormat" /> to <see cref="ExportFormat" />.
    /// </summary>
    public static implicit operator ExportFormat(SessionExportFormat exportFormat)
    {
        return new ExportFormat(value: exportFormat);
    }

    /// <summary>
    /// Implicit conversion from <see cref="string" /> to <see cref="ExportFormat" />.
    /// </summary>
    public static implicit operator ExportFormat(string exportFormat)
    {
        return new ExportFormat(value: exportFormat);
    }
}
