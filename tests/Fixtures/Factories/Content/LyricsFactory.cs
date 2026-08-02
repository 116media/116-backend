using System.Reflection;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="LyricsEntity"/> instances in tests.
/// </summary>
public static class LyricsFactory
{
    /// <summary>
    /// Creates a free lyrics page in Draft status with the given category.
    /// </summary>
    public static LyricsEntity Create(Guid categoryId) => new LyricsBuilder(categoryId).Build();

    /// <summary>
    /// Creates a free lyrics page in Draft status.
    /// </summary>
    public static LyricsEntity CreateFree(Guid categoryId) => new LyricsBuilder(categoryId).Build();

    /// <summary>
    /// Creates a paid lyrics page in Draft status linked to a customer and order item.
    /// </summary>
    public static LyricsEntity CreatePaid(Guid categoryId, Guid customerId, Guid orderItemId) =>
        new LyricsBuilder(categoryId).WithCustomer(customerId, orderItemId).Build();

    /// <summary>
    /// Creates a free lyrics page with a specific ID in Draft status.
    /// </summary>
    public static LyricsEntity CreateWithId(Guid id, Guid categoryId) =>
        new LyricsBuilder(categoryId).WithId(id).Build();

    /// <summary>
    /// Creates a lyrics page linked to a video.
    /// </summary>
    public static LyricsEntity CreateForVideo(Guid categoryId, Guid videoId) =>
        new LyricsBuilder(categoryId).WithVideoId(videoId).Build();

    /// <summary>
    /// Creates a published lyrics page linked to a video with a known slug.
    /// </summary>
    public static LyricsEntity CreatePublishedForVideoWithSlug(Guid categoryId, Guid videoId, string slug) =>
        new LyricsBuilder(categoryId).WithVideoId(videoId).WithSlug(slug).AsPublished().Build();

    /// <summary>
    /// Creates a free lyrics page with a cover image file id set.
    /// </summary>
    public static LyricsEntity CreateWithCoverImageFileId(Guid categoryId, Guid coverImageFileId) =>
        new LyricsBuilder(categoryId).WithCoverImageFileId(coverImageFileId).Build();

    /// <summary>
    /// Creates a free lyrics page with song-credit metadata populated.
    /// </summary>
    public static LyricsEntity CreateWithMetadata(
        Guid categoryId,
        string? album = null,
        short? releaseYear = null,
        string? label = null,
        string? songwriter = null,
        string? producer = null
    ) => new LyricsBuilder(categoryId).WithMetadata(album, releaseYear, label, songwriter, producer).Build();

    /// <summary>
    /// Creates a free lyrics page with the given tags applied.
    /// </summary>
    public static LyricsEntity CreateWithTags(Guid categoryId, params Guid[] tagIds) =>
        new LyricsBuilder(categoryId).WithTags(tagIds).Build();

    /// <summary>
    /// Creates a free lyrics page with specific song title and artist name.
    /// </summary>
    public static LyricsEntity Create(Guid categoryId, string songTitle, string artistName) =>
        new LyricsBuilder(categoryId).WithSongTitle(songTitle).WithArtistName(artistName).Build();

    /// <summary>
    /// Creates a lyrics page with a known valid slug (for slug-conflict tests).
    /// </summary>
    public static LyricsEntity CreateWithSlug(Guid categoryId, string slug) =>
        new LyricsBuilder(categoryId).WithSlug(slug).Build();

    /// <summary>
    /// Creates a lyrics page linked to a real, addressable artist profile.
    /// </summary>
    public static LyricsEntity CreateForArtist(Guid categoryId, Guid artistId) =>
        new LyricsBuilder(categoryId).WithArtistId(artistId).Build();

    /// <summary>
    /// Creates a lyrics page linked to a real, addressable album.
    /// </summary>
    public static LyricsEntity CreateForAlbum(Guid categoryId, Guid albumId) =>
        new LyricsBuilder(categoryId).WithAlbumId(albumId).Build();

    /// <summary>
    /// Creates a published lyrics page linked to a real, addressable artist profile.
    /// </summary>
    public static LyricsEntity CreatePublishedForArtist(Guid categoryId, Guid artistId) =>
        new LyricsBuilder(categoryId).WithArtistId(artistId).AsPublished().Build();

    /// <summary>
    /// Creates a published lyrics page linked to a real, addressable album, used to exercise
    /// the "more from this album" sibling-track lookup.
    /// </summary>
    public static LyricsEntity CreatePublishedForAlbum(Guid categoryId, Guid albumId) =>
        new LyricsBuilder(categoryId).WithAlbumId(albumId).AsPublished().Build();

    /// <summary>
    /// Creates a published lyrics page with the Video navigation property loaded via reflection.
    /// Use this when the test exercises a specification that reads <c>entity.Video.CategoryId</c>.
    /// </summary>
    public static LyricsEntity CreatePublishedWithVideoNavigation(Guid categoryId, VideoEntity video)
    {
        LyricsEntity entity = new LyricsBuilder(categoryId).WithVideoId(video.Id).AsPublished().Build();
        typeof(LyricsEntity).GetProperty("Video", BindingFlags.Public | BindingFlags.Instance)!.SetValue(entity, video);
        return entity;
    }

    /// <summary>
    /// Creates a list of free lyrics pages in Draft status.
    /// </summary>
    public static List<LyricsEntity> CreateMany(Guid categoryId, int count) =>
        Enumerable.Range(0, count).Select(_ => Create(categoryId)).ToList();

    /// <summary>
    /// Creates a list of published lyrics pages.
    /// </summary>
    public static List<LyricsEntity> CreateManyPublished(Guid categoryId, int count) =>
        Enumerable.Range(0, count).Select(_ => CreatePublished(categoryId)).ToList();

    /// <summary>
    /// Creates a published free lyrics page.
    /// </summary>
    public static LyricsEntity CreatePublished(Guid categoryId) => new LyricsBuilder(categoryId).AsPublished().Build();

    /// <summary>
    /// Creates a rejected free lyrics page.
    /// </summary>
    public static LyricsEntity CreateRejected(Guid categoryId) => new LyricsBuilder(categoryId).AsRejected().Build();

    /// <summary>
    /// Creates an approved free lyrics page.
    /// </summary>
    public static LyricsEntity CreateApproved(Guid categoryId) => new LyricsBuilder(categoryId).AsApproved().Build();

    /// <summary>
    /// Creates a free lyrics page in PendingReview status.
    /// </summary>
    public static LyricsEntity CreatePendingReview(Guid categoryId) =>
        new LyricsBuilder(categoryId).AsPendingReview().Build();

    /// <summary>
    /// Creates a paid lyrics page in PendingPayment status.
    /// </summary>
    public static LyricsEntity CreatePendingPayment(Guid categoryId, Guid customerId, Guid orderItemId) =>
        new LyricsBuilder(categoryId).WithCustomer(customerId, orderItemId).AsPendingPayment().Build();

    /// <summary>
    /// Creates an archived free lyrics page.
    /// </summary>
    public static LyricsEntity CreateArchived(Guid categoryId) => new LyricsBuilder(categoryId).AsArchived().Build();

    /// <summary>
    /// Creates a published lyrics page with an active paid promotion stamped.
    /// </summary>
    public static LyricsEntity CreatePromoted(
        Guid categoryId,
        Guid? promotionLevelId = null,
        DateTimeOffset? until = null
    ) => new LyricsBuilder(categoryId).AsPublished().WithPromotion(promotionLevelId, until).Build();

    /// <summary>
    /// Creates a paid lyrics page linked to a customer and order item with an active paid
    /// promotion stamped, without transitioning its editorial status.
    /// </summary>
    public static LyricsEntity CreatePaidWithPromotion(
        Guid categoryId,
        Guid customerId,
        Guid orderItemId,
        Guid? promotionLevelId = null,
        DateTimeOffset? until = null
    ) =>
        new LyricsBuilder(categoryId)
            .WithCustomer(customerId, orderItemId)
            .WithPromotion(promotionLevelId, until)
            .Build();

    /// <summary>
    /// Creates a published lyrics page with a specific song title, used to prove recency-based
    /// ("newest") sort order wins over alphabetical order. Callers that need a specific
    /// <c>CreatedAt</c> must backdate it afterward via a raw SQL update, since the audit
    /// interceptor always overwrites the entity's in-memory <c>CreatedAt</c> on insert.
    /// </summary>
    public static LyricsEntity CreatePublishedWithSongTitle(Guid categoryId, string songTitle) =>
        new LyricsBuilder(categoryId).WithSongTitle(songTitle).AsPublished().Build();

    /// <summary>
    /// Creates a free lyrics page with the default known values from TestConstants.
    /// </summary>
    public static LyricsEntity CreateDefault(Guid categoryId) =>
        new LyricsBuilder(categoryId)
            .WithSongTitle(TestConstants.Content.Editorial.Lyrics.ValidSongTitle)
            .WithArtistName(TestConstants.Content.Editorial.Lyrics.ValidArtistName)
            .WithSlug(TestConstants.Content.Editorial.Lyrics.ValidSlug)
            .Build();
}
