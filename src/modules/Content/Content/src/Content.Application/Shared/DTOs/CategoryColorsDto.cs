namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// A ready-to-use, accessible color pair derived from a category's poster image,
/// exposed so the frontend can render show cards without parsing or computing
/// colors. Both values are plain <c>#RRGGBB</c> hex, ready to drop straight into
/// CSS.
/// </summary>
/// <param name="Background">The poster's dominant color, used as the card background.</param>
/// <param name="Foreground">The contrasting text color (black or white) for the background.</param>
public record CategoryColorsDto(string Background, string Foreground);
