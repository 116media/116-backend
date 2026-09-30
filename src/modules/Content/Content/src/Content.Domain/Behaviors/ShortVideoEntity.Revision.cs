using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Revision behaviour of <see cref="ShortVideoEntity" />. Its state lives in <c>Entities/ShortVideoEntity.cs</c>.
/// </summary>
public partial class ShortVideoEntity
{
    /// <summary>
    /// Updates the editable metadata fields of this short video.
    /// Slug is immutable after creation to preserve public URLs.
    /// </summary>
    /// <param name="title">The new display title.</param>
    /// <param name="videoId">Optional parent full video ID. <c>null</c> to make standalone.</param>
    public void Update(string title, Guid? videoId)
    {
        if (string.IsNullOrWhiteSpace(value: title))
        {
            throw new ContentRuleException(ContentRuleCodes.ShortVideoTitleRequired);
        }

        Title = title;
        VideoId = videoId;
        HasFullVideo = videoId.HasValue;
        AddDomainEvent(new ShortVideoChangedEvent(ShortVideoId: Id));
    }

    /// <summary>
    /// Replaces the video file reference after a successful re-upload.
    /// </summary>
    /// <param name="videoFileId">
    /// The new FileEntity ID for the re-uploaded video file.
    /// </param>
    public void ReplaceVideoFile(Guid videoFileId)
    {
        VideoFileId = videoFileId;
        AddDomainEvent(new ShortVideoChangedEvent(ShortVideoId: Id));
    }

    /// <summary>
    /// Sets or replaces the thumbnail file reference for this short video.
    /// </summary>
    /// <param name="thumbnailFileId">
    /// The FileEntity ID for the uploaded thumbnail, or null to clear it.
    /// </param>
    public void SetThumbnailFileId(Guid? thumbnailFileId)
    {
        ThumbnailFileId = thumbnailFileId;
        AddDomainEvent(new ShortVideoChangedEvent(ShortVideoId: Id));
    }
}
