using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="LyricsEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; LyricsFactory only names chains three or more tests share.
/// </summary>
public class LyricsBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _categoryId;
    private string _songTitle = TestConstants.Lyrics.ValidSongTitle;
    private string _artistName = TestConstants.Lyrics.ValidArtistName;
    private string _lyricsText = TestConstants.Lyrics.ValidLyricsText;
    private string _language = TestConstants.Lyrics.ValidLanguage;
    private string _slug = $"{TestConstants.Lyrics.ValidSlug}-{Guid.NewGuid():N}";
    private Guid _authorId = Guid.NewGuid();
    private Guid? _videoId;
    private Guid? _customerId;
    private Guid? _orderItemId;
    private Guid? _artistId;
    private Guid? _albumId;
    private EnumContentStatus _targetStatus = EnumContentStatus.Draft;
    private string? _rejectionReason;
    private readonly List<Guid> _tagIds = [];
    private (Guid PromotionLevelId, DateTimeOffset Until)? _promotion;

    /// <summary>
    /// Initializes a new instance of the <see cref="LyricsBuilder"/> class with a required category ID.
    /// </summary>
    public LyricsBuilder(Guid categoryId)
    {
        _categoryId = categoryId;
    }

    /// <summary>
    /// Sets the lyrics slug.
    /// </summary>
    public LyricsBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>
    /// Sets the song title.
    /// </summary>
    public LyricsBuilder WithSongTitle(string songTitle)
    {
        _songTitle = songTitle;
        return this;
    }

    /// <summary>
    /// Sets the artist name.
    /// </summary>
    public LyricsBuilder WithArtistName(string artistName)
    {
        _artistName = artistName;
        return this;
    }

    /// <summary>
    /// Sets the language code.
    /// </summary>
    public LyricsBuilder WithLanguage(string language)
    {
        _language = language;
        return this;
    }

    /// <summary>
    /// Links this lyrics page to a video.
    /// </summary>
    public LyricsBuilder WithVideoId(Guid videoId)
    {
        _videoId = videoId;
        return this;
    }

    /// <summary>
    /// Makes the lyrics page a paid page linked to a customer and order item.
    /// </summary>
    public LyricsBuilder WithCustomer(Guid customerId, Guid orderItemId)
    {
        _customerId = customerId;
        _orderItemId = orderItemId;
        return this;
    }

    /// <summary>
    /// Transitions the lyrics page to PendingPayment status.
    /// </summary>
    public LyricsBuilder AsPendingPayment()
    {
        _targetStatus = EnumContentStatus.PendingPayment;
        return this;
    }

    /// <summary>
    /// Transitions the lyrics page to PendingReview status.
    /// </summary>
    public LyricsBuilder AsPendingReview()
    {
        _targetStatus = EnumContentStatus.PendingReview;
        return this;
    }

    /// <summary>
    /// Transitions the lyrics page to Approved status.
    /// </summary>
    public LyricsBuilder AsApproved()
    {
        _targetStatus = EnumContentStatus.Approved;
        return this;
    }

    /// <summary>
    /// Transitions the lyrics page to Published status.
    /// </summary>
    public LyricsBuilder AsPublished()
    {
        _targetStatus = EnumContentStatus.Published;
        return this;
    }

    /// <summary>
    /// Transitions the lyrics page to Rejected status with a reason.
    /// </summary>
    public LyricsBuilder AsRejected(string? reason = null)
    {
        _targetStatus = EnumContentStatus.Rejected;
        _rejectionReason = reason ?? TestConstants.Lyrics.ValidRejectionReason;
        return this;
    }

    /// <summary>
    /// Transitions the lyrics page to Archived status.
    /// </summary>
    public LyricsBuilder AsArchived()
    {
        _targetStatus = EnumContentStatus.Archived;
        return this;
    }

    /// <summary>
    /// Links this lyrics page to a real, addressable artist profile.
    /// </summary>
    public LyricsBuilder WithArtistId(Guid artistId)
    {
        _artistId = artistId;
        return this;
    }

    /// <summary>
    /// Links this lyrics page to a real, addressable album.
    /// </summary>
    public LyricsBuilder WithAlbumId(Guid albumId)
    {
        _albumId = albumId;
        return this;
    }

    /// <summary>
    /// Applies the given tag ids to the lyrics page via <see cref="LyricsTagEntity"/> associations.
    /// </summary>
    public LyricsBuilder WithTags(params Guid[] tagIds)
    {
        _tagIds.AddRange(tagIds);
        return this;
    }

    /// <summary>
    /// Stamps an active paid promotion on the lyrics page via <see cref="LyricsEntity.StampPromotion"/>.
    /// </summary>
    public LyricsBuilder WithPromotion(Guid? promotionLevelId = null, DateTimeOffset? until = null)
    {
        _promotion = (promotionLevelId ?? Guid.NewGuid(), until ?? DateTimeOffset.UtcNow.AddDays(7));
        return this;
    }

    /// <summary>
    /// Builds the <see cref="LyricsEntity"/> instance.
    /// </summary>
    public LyricsEntity Build()
    {
        LyricsEntity entity = _customerId.HasValue
            ? LyricsEntity.CreatePaid(
                id: _id,
                customerId: _customerId.Value,
                orderItemId: _orderItemId!.Value,
                categoryId: _categoryId,
                videoId: _videoId,
                songTitle: _songTitle,
                artistName: _artistName,
                lyricsText: _lyricsText,
                language: _language,
                slug: _slug,
                authorId: _authorId
            )
            : LyricsEntity.CreateFree(
                id: _id,
                categoryId: _categoryId,
                videoId: _videoId,
                songTitle: _songTitle,
                artistName: _artistName,
                lyricsText: _lyricsText,
                language: _language,
                slug: _slug,
                authorId: _authorId
            );

        ApplyStatusTransition(entity);

        foreach (Guid tagId in _tagIds)
        {
            entity.Tags.Add(LyricsTagEntity.Create(Guid.NewGuid(), entity.Id, tagId));
        }

        if (_artistId.HasValue)
        {
            entity.LinkArtist(_artistId.Value);
        }

        if (_albumId.HasValue)
        {
            entity.LinkAlbum(_albumId.Value);
        }

        if (_promotion.HasValue)
        {
            entity.StampPromotion(_promotion.Value.PromotionLevelId, _promotion.Value.Until);
        }

        entity.CreatedAt = DateTime.UtcNow;

        return entity;
    }

    private void ApplyStatusTransition(LyricsEntity entity)
    {
        switch (_targetStatus)
        {
            case EnumContentStatus.PendingPayment:
                entity.Submit();
                break;
            case EnumContentStatus.PendingReview:
                entity.MarkPendingReview();
                break;
            case EnumContentStatus.Approved:
                entity.MarkPendingReview();
                entity.Approve();
                break;
            case EnumContentStatus.Published:
                entity.MarkPendingReview();
                entity.Approve();
                entity.Publish();
                break;
            case EnumContentStatus.Rejected:
                entity.MarkPendingReview();
                entity.Reject(_rejectionReason ?? TestConstants.Lyrics.ValidRejectionReason);
                break;
            case EnumContentStatus.Archived:
                entity.MarkPendingReview();
                entity.Approve();
                entity.Publish();
                entity.Archive();
                break;
            case EnumContentStatus.Draft:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
