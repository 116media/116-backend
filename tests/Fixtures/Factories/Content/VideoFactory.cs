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
    /// <summary>Creates a free video in Draft status with the given category.</summary>
    public static VideoEntity Create(Guid categoryId) => new VideoBuilder(categoryId).Build();

    /// <summary>Creates a free video with a YouTube ID attached (Draft status).</summary>
    public static VideoEntity CreateWithYoutubeId(Guid categoryId) =>
        new VideoBuilder(categoryId).WithYoutubeId().Build();

    /// <summary>Creates a paid video in Draft status.</summary>
    public static VideoEntity CreatePaid(Guid categoryId, Guid customerId, Guid orderItemId) =>
        new VideoBuilder(categoryId).WithCustomer(customerId, orderItemId).Build();

    /// <summary>Creates a free video with a specific ID in Draft status.</summary>
    public static VideoEntity CreateWithId(Guid id, Guid categoryId) => new VideoBuilder(categoryId).WithId(id).Build();

    /// <summary>Creates a published free video (requires YouTube ID).</summary>
    public static VideoEntity CreatePublished(Guid categoryId) => new VideoBuilder(categoryId).AsPublished().Build();

    /// <summary>Creates a rejected free video.</summary>
    public static VideoEntity CreateRejected(Guid categoryId) => new VideoBuilder(categoryId).AsRejected().Build();

    /// <summary>Creates an approved free video.</summary>
    public static VideoEntity CreateApproved(Guid categoryId) => new VideoBuilder(categoryId).AsApproved().Build();

    /// <summary>Creates a video with a known slug.</summary>
    public static VideoEntity CreateWithSlug(Guid categoryId, string slug) =>
        new VideoBuilder(categoryId).WithSlug(slug).Build();

    /// <summary>Creates a list of free videos in Draft status.</summary>
    public static List<VideoEntity> CreateMany(Guid categoryId, int count) =>
        Enumerable.Range(0, count).Select(_ => Create(categoryId)).ToList();

    /// <summary>Creates a list of published videos.</summary>
    public static List<VideoEntity> CreateManyPublished(Guid categoryId, int count) =>
        Enumerable.Range(0, count).Select(_ => CreatePublished(categoryId)).ToList();

    /// <summary>Creates a featured published video with a future expiry.</summary>
    public static VideoEntity CreateFeatured(Guid categoryId)
    {
        VideoEntity entity = new VideoBuilder(categoryId).AsPublished().Build();
        entity.StampFeatured(DateTimeOffset.UtcNow.AddDays(7));
        return entity;
    }

    /// <summary>Creates a free video in PendingReview status.</summary>
    public static VideoEntity CreatePendingReview(Guid categoryId) =>
        new VideoBuilder(categoryId).AsPendingReview().Build();

    /// <summary>Creates a paid video in PendingPayment status.</summary>
    public static VideoEntity CreatePendingPayment(Guid categoryId, Guid customerId, Guid orderItemId) =>
        new VideoBuilder(categoryId).WithCustomer(customerId, orderItemId).AsPendingPayment().Build();

    /// <summary>Creates an archived free video.</summary>
    public static VideoEntity CreateArchived(Guid categoryId) => new VideoBuilder(categoryId).AsArchived().Build();

    /// <summary>Creates a free video with a thumbnail set (for delete cleanup tests).</summary>
    public static VideoEntity CreateWithThumbnail(Guid categoryId) =>
        new VideoBuilder(categoryId).WithThumbnail().Build();

    /// <summary>Creates an approved free video with a YouTube ID attached (ready to publish).</summary>
    public static VideoEntity CreateApprovedWithYoutubeId(Guid categoryId) =>
        new VideoBuilder(categoryId).WithYoutubeId().AsApproved().Build();

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

    /// <summary>Creates a list of free videos with the Category navigation property loaded via reflection.</summary>
    public static List<VideoEntity> CreateManyWithCategory(Guid categoryId, CategoryEntity category, int count) =>
        Enumerable.Range(0, count).Select(_ => CreateWithCategory(categoryId, category)).ToList();

    /// <summary>Creates a free video with the default known values from TestConstants.</summary>
    public static VideoEntity CreateDefault(Guid categoryId) =>
        new VideoBuilder(categoryId)
            .WithTitle(TestConstants.Content.Editorial.Video.ValidTitle)
            .WithSlug(TestConstants.Content.Editorial.Video.ValidSlug)
            .Build();
}
