using System.Reflection;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="VideoEntity"/> instances in tests.
/// </summary>
public static class VideoFactory
{
    /// <summary>
    /// Creates a free video in Draft status with the given category.
    /// </summary>
    public static VideoEntity Create(Guid categoryId) => new VideoBuilder(categoryId).Build();

    /// <summary>
    /// Creates a free video with a YouTube URL attached (Draft status).
    /// </summary>
    public static VideoEntity CreateWithYoutubeUrl(Guid categoryId) =>
        new VideoBuilder(categoryId).WithYoutubeUrl().Build();

    /// <summary>
    /// Creates a paid video in Draft status.
    /// </summary>
    public static VideoEntity CreatePaid(Guid categoryId, Guid customerId, Guid orderItemId) =>
        new VideoBuilder(categoryId).WithCustomer(customerId, orderItemId).Build();

    /// <summary>
    /// Creates a free video with a specific ID in Draft status.
    /// </summary>
    public static VideoEntity CreateWithId(Guid id, Guid categoryId) => new VideoBuilder(categoryId).WithId(id).Build();

    /// <summary>
    /// Creates a published free video (requires YouTube URL).
    /// </summary>
    public static VideoEntity CreatePublished(Guid categoryId) => new VideoBuilder(categoryId).AsPublished().Build();

    /// <summary>
    /// Creates a published free video with an explicit PublishedAt, for deterministic ordering.
    /// </summary>
    public static VideoEntity CreatePublishedAt(Guid categoryId, DateTimeOffset publishedAt) =>
        new VideoBuilder(categoryId).AsPublishedAt(publishedAt).Build();

    /// <summary>
    /// Creates a rejected free video.
    /// </summary>
    public static VideoEntity CreateRejected(Guid categoryId) => new VideoBuilder(categoryId).AsRejected().Build();

    /// <summary>
    /// Creates an approved free video.
    /// </summary>
    public static VideoEntity CreateApproved(Guid categoryId) => new VideoBuilder(categoryId).AsApproved().Build();

    /// <summary>
    /// Creates a video with a known slug.
    /// </summary>
    public static VideoEntity CreateWithSlug(Guid categoryId, string slug) =>
        new VideoBuilder(categoryId).WithSlug(slug).Build();

    /// <summary>
    /// Creates a video linked to a real, addressable artist profile.
    /// </summary>
    public static VideoEntity CreateForArtist(Guid categoryId, Guid artistId) =>
        new VideoBuilder(categoryId).WithArtistId(artistId).Build();

    /// <summary>
    /// Creates a published video linked to a real, addressable artist profile.
    /// </summary>
    public static VideoEntity CreatePublishedForArtist(Guid categoryId, Guid artistId) =>
        new VideoBuilder(categoryId).WithArtistId(artistId).AsPublished().Build();

    /// <summary>
    /// Creates a list of free videos in Draft status.
    /// </summary>
    public static List<VideoEntity> CreateMany(Guid categoryId, int count) =>
        Enumerable.Range(0, count).Select(_ => Create(categoryId)).ToList();

    /// <summary>
    /// Creates a list of published videos.
    /// </summary>
    public static List<VideoEntity> CreateManyPublished(Guid categoryId, int count) =>
        Enumerable.Range(0, count).Select(_ => CreatePublished(categoryId)).ToList();

    /// <summary>
    /// Creates a promoted published video with a future expiry. Pass
    /// <paramref name="promotionLevelId" /> referencing a seeded promotion level to satisfy
    /// the foreign key when the video is persisted.
    /// </summary>
    public static VideoEntity CreatePromoted(Guid categoryId, Guid? promotionLevelId = null) =>
        new VideoBuilder(categoryId)
            .AsPublished()
            .AsPromoted(DateTimeOffset.UtcNow.AddDays(7), promotionLevelId)
            .Build();

    /// <summary>
    /// Creates a free video in PendingReview status.
    /// </summary>
    public static VideoEntity CreatePendingReview(Guid categoryId) =>
        new VideoBuilder(categoryId).AsPendingReview().Build();

    /// <summary>
    /// Creates a paid video in PendingPayment status.
    /// </summary>
    public static VideoEntity CreatePendingPayment(Guid categoryId, Guid customerId, Guid orderItemId) =>
        new VideoBuilder(categoryId).WithCustomer(customerId, orderItemId).AsPendingPayment().Build();

    /// <summary>
    /// Creates an archived free video.
    /// </summary>
    public static VideoEntity CreateArchived(Guid categoryId) => new VideoBuilder(categoryId).AsArchived().Build();

    /// <summary>
    /// Creates a free video with a thumbnail set (for delete cleanup tests).
    /// </summary>
    public static VideoEntity CreateWithThumbnail(Guid categoryId) =>
        new VideoBuilder(categoryId).WithThumbnail().Build();

    /// <summary>
    /// Creates an approved free video with a YouTube URL attached (ready to publish).
    /// </summary>
    public static VideoEntity CreateApprovedWithYoutubeUrl(Guid categoryId) =>
        new VideoBuilder(categoryId).WithYoutubeUrl().AsApproved().Build();

    /// <summary>
    /// Creates a free video with the Category navigation property loaded via reflection.
    /// Use this when the test exercises a mapper that accesses <c>entity.Category.Name</c>.
    /// </summary>
    public static VideoEntity CreateWithCategory(Guid categoryId, CategoryEntity category)
    {
        VideoEntity entity = Create(categoryId);
        typeof(VideoEntity)
            .GetProperty("Category", BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(entity, category);
        return entity;
    }

    /// <summary>
    /// Creates a list of free videos with the Category navigation property loaded via reflection.
    /// </summary>
    public static List<VideoEntity> CreateManyWithCategory(Guid categoryId, CategoryEntity category, int count) =>
        Enumerable.Range(0, count).Select(_ => CreateWithCategory(categoryId, category)).ToList();

    /// <summary>
    /// Creates a free video with the default known values from TestConstants.
    /// </summary>
    public static VideoEntity CreateDefault(Guid categoryId) =>
        new VideoBuilder(categoryId)
            .WithTitle(TestConstants.Content.Editorial.Video.ValidTitle)
            .WithSlug(TestConstants.Content.Editorial.Video.ValidSlug)
            .Build();

    /// <summary>
    /// Creates a free video in Draft status with a specific title.
    /// </summary>
    public static VideoEntity CreateWithTitle(Guid categoryId, string title) =>
        new VideoBuilder(categoryId).WithTitle(title).Build();

    /// <summary>
    /// Creates a free video with a shooting scheduled in the future.
    /// A YouTube URL cannot be attached until after the shoot date passes.
    /// </summary>
    public static VideoEntity CreateWithFutureShoot(Guid categoryId, int daysFromNow = 30) =>
        new VideoBuilder(categoryId).WithShootingScheduledAt(DateTimeOffset.UtcNow.AddDays(daysFromNow)).Build();

    /// <summary>
    /// Creates a free video with a shooting that already happened (in the past).
    /// A YouTube URL can be freely attached.
    /// </summary>
    public static VideoEntity CreateWithPastShoot(Guid categoryId, int daysAgo = 7) =>
        new VideoBuilder(categoryId).WithShootingScheduledAt(DateTimeOffset.UtcNow.AddDays(-daysAgo)).Build();
}
