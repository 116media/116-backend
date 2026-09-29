namespace _116.Content.Domain.Enums;

/// <summary>
/// Classifies an <see cref="Entities.AlbumEntity" /> by release kind. The artist profile
/// renders <see cref="Album" /> and <see cref="Mixtape" /> as two separate sections;
/// <see cref="EP" /> and <see cref="Single" /> exist from day one because adding an enum
/// member later is one line while re-classifying live rows is manual catalog work — but
/// neither is surfaced anywhere yet, and both sections filter on explicit values so a new
/// member never leaks into an existing heading.
/// </summary>
public enum EnumReleaseType
{
    /// <summary>
    /// A full-length studio album. The default for rows that predate the discriminator.
    /// </summary>
    Album,

    /// <summary>
    /// A mixtape, rendered as its own section on the artist profile.
    /// </summary>
    Mixtape,

    /// <summary>
    /// An extended play. Not surfaced in the UI yet.
    /// </summary>
    EP,

    /// <summary>
    /// A standalone single release. Not surfaced in the UI yet.
    /// </summary>
    Single,
}
