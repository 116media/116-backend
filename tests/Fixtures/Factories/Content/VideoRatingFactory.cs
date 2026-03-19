using _116.Content.Domain.Entities;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="VideoRatingEntity"/> instances in tests.
/// </summary>
public static class VideoRatingFactory
{
    /// <summary>Creates a video rating with the given parameters.</summary>
    public static VideoRatingEntity Create(Guid videoId, Guid userId, short stars = 5) =>
        VideoRatingEntity.Create(id: Guid.NewGuid(), userId: userId, videoId: videoId, stars: stars);

    /// <summary>Creates a video rating with a specific ID.</summary>
    public static VideoRatingEntity CreateWithId(Guid id, Guid videoId, Guid userId, short stars = 5) =>
        VideoRatingEntity.Create(id: id, userId: userId, videoId: videoId, stars: stars);

    /// <summary>Creates a list of video ratings for the given video.</summary>
    public static IReadOnlyList<VideoRatingEntity> CreateMany(int count, Guid videoId, Guid userId, short stars = 5) =>
        Enumerable.Range(0, count).Select(_ => Create(videoId, userId, stars)).ToList();
}
