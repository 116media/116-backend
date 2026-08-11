using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="ShortVideoEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; ShortVideoFactory only names chains three or more tests share.
/// </summary>
public class ShortVideoBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _title = $"{TestConstants.ShortVideo.ValidTitle} {Guid.NewGuid():N}";
    private string _slug = $"{TestConstants.ShortVideo.ValidSlug}-{Guid.NewGuid():N}";
    private Guid _authorId = Guid.NewGuid();
    private Guid? _videoFileId = Guid.NewGuid();
    private Guid? _videoId;
    private Guid? _thumbnailFileId;
    private bool _isInactive;

    /// <summary>
    /// Sets the short video title.
    /// </summary>
    public ShortVideoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Sets the short video slug.
    /// </summary>
    public ShortVideoBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>
    /// Sets the author ID.
    /// </summary>
    public ShortVideoBuilder WithAuthorId(Guid authorId)
    {
        _authorId = authorId;
        return this;
    }

    /// <summary>
    /// Builds the short video as a file-less draft, simulating a short video created before its
    /// video file has been uploaded. Such drafts cannot be activated and are hidden from the feed.
    /// </summary>
    public ShortVideoBuilder WithoutVideoFile()
    {
        _videoFileId = null;
        return this;
    }

    /// <summary>
    /// Links this short video to a parent full video (teaser mode).
    /// </summary>
    public ShortVideoBuilder AsTeaser(Guid videoId)
    {
        _videoId = videoId;
        return this;
    }

    /// <summary>
    /// Sets a thumbnail file ID to simulate an uploaded thumbnail.
    /// </summary>
    public ShortVideoBuilder WithThumbnail()
    {
        _thumbnailFileId = Guid.NewGuid();
        return this;
    }

    /// <summary>
    /// Marks the short video as inactive.
    /// </summary>
    public ShortVideoBuilder AsInactive()
    {
        _isInactive = true;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="ShortVideoEntity"/> instance.
    /// </summary>
    public ShortVideoEntity Build()
    {
        var errors = TestErrorsFactory.CreateShortVideoErrors();
        ShortVideoEntity entity = _videoId.HasValue
            ? ShortVideoEntity.CreateTeaser(
                id: _id,
                title: _title,
                slug: _slug,
                videoId: _videoId.Value,
                authorId: _authorId,
                errors: errors
            )
            : ShortVideoEntity.CreateStandalone(
                id: _id,
                title: _title,
                slug: _slug,
                authorId: _authorId,
                errors: errors
            );

        if (_videoFileId.HasValue)
        {
            entity.ReplaceVideoFile(videoFileId: _videoFileId.Value);
        }

        if (_thumbnailFileId.HasValue)
        {
            entity.SetThumbnailFileId(_thumbnailFileId);
        }

        if (!_isInactive && _videoFileId.HasValue)
        {
            entity.Activate(errors);
        }

        return entity;
    }
}
