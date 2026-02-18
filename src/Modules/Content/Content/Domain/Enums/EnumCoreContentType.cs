namespace _116.Content.Domain.Enums;

/// <summary>
/// Defines the core content types within the platform.
/// These are immutable structural constants seeded once at startup.
/// Every category, article, video, and short must belong to one of these types.
/// </summary>
public enum EnumCoreContentType
{
    /// <summary>
    /// Written editorial content: artist profiles, reviews, gossip, spotlights, lyrics pages.
    /// </summary>
    Article,

    /// <summary>
    /// Long-form video productions embedded from YouTube: music videos, interviews, documentaries, podcasts.
    /// Requires a YouTube link before publishing.
    /// </summary>
    Video,

    /// <summary>
    /// Short-form loopable video clips: teasers, reels, and quick previews.
    /// </summary>
    Short,
}
