using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Lifecycle transitions of <see cref="ShortVideoEntity" />. Its state lives in <c>Entities/ShortVideoEntity.cs</c>.
/// </summary>
public partial class ShortVideoEntity
{
    /// <summary>
    /// Makes the short video visible on the public feed. A short video cannot be activated
    /// until its video file has been uploaded.
    /// </summary>
    /// <returns><c>true</c> if activated; <c>false</c> if already active.</returns>
    public bool Activate()
    {
        if (VideoFileId is null)
        {
            throw new ContentRuleException(ContentRuleCodes.ShortVideoFileRequired);
        }

        if (IsActive)
        {
            return false;
        }

        IsActive = true;
        AddDomainEvent(new ShortVideoChangedEvent(ShortVideoId: Id));

        return true;
    }

    /// <summary>
    /// Hides the short video from the public feed. Deactivation is reversible
    /// and does not delete any media assets.
    /// </summary>
    /// <returns><c>true</c> if deactivated; <c>false</c> if already inactive.</returns>
    public bool Deactivate()
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = false;
        AddDomainEvent(new ShortVideoChangedEvent(ShortVideoId: Id));

        return true;
    }

    /// <summary>
    /// Declares the short video's removal, capturing the video and thumbnail
    /// file ids before the row disappears so post-commit consumers can clean
    /// the remote assets without re-querying deleted rows. Called by the
    /// delete flow immediately before the repository removal.
    /// </summary>
    public void MarkDeleted()
    {
        AddDomainEvent(
            new ShortVideoDeletedEvent(ShortVideoId: Id, VideoFileId: VideoFileId, ThumbnailFileId: ThumbnailFileId)
        );
    }
}
