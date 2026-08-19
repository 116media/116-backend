using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when a YouTube video URL is attached to a video. The URL is the
/// attached fact itself and rides the payload so the post-commit consumer can
/// download the YouTube thumbnail and attach it without holding the video
/// transaction open across external calls. A thumbnail outage no longer fails
/// the attach command; the video renders thumbnail-less until the handler
/// lands the asset.
/// </summary>
/// <param name="VideoId">The video the URL was attached to.</param>
/// <param name="YoutubeVideoUrl">The full YouTube video URL that was attached.</param>
public record VideoYoutubeUrlAttachedEvent(Guid VideoId, string YoutubeVideoUrl) : IDomainEvent;
