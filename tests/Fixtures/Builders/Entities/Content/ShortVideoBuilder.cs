using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="ShortVideoEntity"/> instances in tests.
/// For test code, prefer using ShortVideoFactory instead of direct Builder usage.
/// </summary>
internal class ShortVideoBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _title = TestConstants.Content.Editorial.ShortVideo.ValidTitle;
    private string _slug = TestConstants.Content.Editorial.ShortVideo.ValidSlug;
    private string _videoUrl = TestConstants.Content.Editorial.ShortVideo.ValidVideoUrl;
    private string _videoStorageKey = TestConstants.Content.Editorial.ShortVideo.ValidVideoStorageKey;
    private Guid _authorId = Guid.NewGuid();
    private Guid? _videoId;
    private string? _thumbnailUrl;
    private string? _thumbnailStorageKey;
    private bool _isInactive;

    /// <summary>Sets the short video ID.</summary>
    public ShortVideoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>Sets the short video title.</summary>
    public ShortVideoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>Sets the short video slug.</summary>
    public ShortVideoBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>Sets the video CDN URL.</summary>
    public ShortVideoBuilder WithVideoUrl(string videoUrl)
    {
        _videoUrl = videoUrl;
        return this;
    }

    /// <summary>Sets the video storage key.</summary>
    public ShortVideoBuilder WithVideoStorageKey(string storageKey)
    {
        _videoStorageKey = storageKey;
        return this;
    }

    /// <summary>Sets the author ID.</summary>
    public ShortVideoBuilder WithAuthorId(Guid authorId)
    {
        _authorId = authorId;
        return this;
    }

    /// <summary>Links this short video to a parent full video (teaser mode).</summary>
    public ShortVideoBuilder AsTeaser(Guid videoId)
    {
        _videoId = videoId;
        return this;
    }

    /// <summary>Sets a thumbnail URL and storage key.</summary>
    public ShortVideoBuilder WithThumbnail(string thumbnailUrl, string thumbnailStorageKey)
    {
        _thumbnailUrl = thumbnailUrl;
        _thumbnailStorageKey = thumbnailStorageKey;
        return this;
    }

    /// <summary>Marks the short video as inactive.</summary>
    public ShortVideoBuilder AsInactive()
    {
        _isInactive = true;
        return this;
    }

    /// <summary>Builds the <see cref="ShortVideoEntity"/> instance.</summary>
    public ShortVideoEntity Build()
    {
        ShortVideoEntity entity = _videoId.HasValue
            ? ShortVideoEntity.CreateTeaser(
                id: _id,
                title: _title,
                slug: _slug,
                videoUrl: _videoUrl,
                videoStorageKey: _videoStorageKey,
                videoId: _videoId.Value,
                authorId: _authorId
            )
            : ShortVideoEntity.CreateStandalone(
                id: _id,
                title: _title,
                slug: _slug,
                videoUrl: _videoUrl,
                videoStorageKey: _videoStorageKey,
                authorId: _authorId
            );

        if (_thumbnailUrl is not null && _thumbnailStorageKey is not null)
        {
            entity.UpdateThumbnail(_thumbnailUrl, _thumbnailStorageKey);
        }

        if (_isInactive)
        {
            entity.Deactivate();
        }

        return entity;
    }
}
