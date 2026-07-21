using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="LyricsEntity"/> instances in tests.
/// For test code, prefer using LyricsFactory instead of direct Builder usage.
/// </summary>
internal class LyricsBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _categoryId;
    private string _songTitle = TestConstants.Content.Editorial.Lyrics.ValidSongTitle;
    private string _artistName = TestConstants.Content.Editorial.Lyrics.ValidArtistName;
    private string _lyricsText = TestConstants.Content.Editorial.Lyrics.ValidLyricsText;
    private string _language = TestConstants.Content.Editorial.Lyrics.ValidLanguage;
    private string _slug = $"{TestConstants.Content.Editorial.Lyrics.ValidSlug}-{Guid.NewGuid():N}";
    private Guid _authorId = Guid.NewGuid();
    private Guid? _videoId;
    private Guid? _customerId;
    private Guid? _orderItemId;
    private Guid? _artistId;
    private Guid? _albumId;
    private EnumContentStatus _targetStatus = EnumContentStatus.Draft;
    private string? _rejectionReason;
    private Guid? _coverImageFileId;
    private string? _album;
    private short? _releaseYear;
    private string? _label;
    private string? _songwriter;
    private string? _producer;
    private readonly List<Guid> _tagIds = [];
    private (Guid PromotionLevelId, DateTimeOffset Until)? _promotion;
    private DateTime? _createdAt;

    /// <summary>
    /// Initializes a new instance of the <see cref="LyricsBuilder"/> class with a required category ID.
    /// </summary>
    public LyricsBuilder(Guid categoryId)
    {
        _categoryId = categoryId;
    }

    /// <summary>
    /// Sets the lyrics ID.
    /// </summary>
    public LyricsBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the category ID.
    /// </summary>
    public LyricsBuilder WithCategoryId(Guid categoryId)
    {
        _categoryId = categoryId;
        return this;
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
    /// Sets the author ID.
    /// </summary>
    public LyricsBuilder WithAuthorId(Guid authorId)
    {
        _authorId = authorId;
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
    /// Sets the lyrics text.
    /// </summary>
    public LyricsBuilder WithLyricsText(string lyricsText)
    {
        _lyricsText = lyricsText;
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
        _rejectionReason = reason ?? TestConstants.Content.Editorial.Lyrics.ValidRejectionReason;
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
    /// Sets the cover/album art file id.
    /// </summary>
    public LyricsBuilder WithCoverImageFileId(Guid coverImageFileId)
    {
        _coverImageFileId = coverImageFileId;
        return this;
    }

    /// <summary>
    /// Sets the song-credit metadata fields (album, release year, label, songwriter, producer)
    /// in a single call. Any parameter left null stays unset.
    /// </summary>
    public LyricsBuilder WithMetadata(
        string? album = null,
        short? releaseYear = null,
        string? label = null,
        string? songwriter = null,
        string? producer = null
    )
    {
        _album = album;
        _releaseYear = releaseYear;
        _label = label;
        _songwriter = songwriter;
        _producer = producer;
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
    /// Overrides the <c>CreatedAt</c> timestamp — used to exercise recency-based sort ordering.
    /// </summary>
    public LyricsBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="LyricsEntity"/> instance.
    /// </summary>
    public LyricsEntity Build()
    {
        var errors = TestErrorsFactory.CreateLyricsErrors();
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
                authorId: _authorId,
                errors: errors
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
                authorId: _authorId,
                errors: errors
            );

        ApplyStatusTransition(entity);

        if (_coverImageFileId.HasValue)
        {
            entity.SetCoverImageFileId(_coverImageFileId.Value);
        }

        if (
            _album is not null
            || _releaseYear is not null
            || _label is not null
            || _songwriter is not null
            || _producer is not null
        )
        {
            entity.UpdateMetadata(_album, _releaseYear, _label, _songwriter, _producer);
        }

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

        entity.CreatedAt = _createdAt ?? DateTime.UtcNow;

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
                entity.Reject(_rejectionReason ?? TestConstants.Content.Editorial.Lyrics.ValidRejectionReason);
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
