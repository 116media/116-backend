using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed.Contracts;

/// <summary>
/// A pinned video category and the latest published videos that fill its feed entry.
/// </summary>
/// <param name="Category">The pinned category.</param>
/// <param name="Videos">Its latest published videos, capped per entry.</param>
public record VideoFeed(CategoryEntity Category, IReadOnlyList<VideoEntity> Videos);

/// <summary>
/// Resolves the video feed: the pinned video categories and their latest videos.
/// </summary>
public interface IPublicVideoFeedService
{
    /// <summary>
    /// Loads the pinned video categories and the latest published videos of each.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<IReadOnlyList<VideoFeed>> GetPinnedFeedsAsync(CancellationToken cancellationToken);
}
