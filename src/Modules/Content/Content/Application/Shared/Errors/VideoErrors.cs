using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Video domain error factory providing simple, readable exception creation.
/// Usage: VideoErrors.NotFound(id) or VideoErrors.SlugAlreadyExists(slug)
/// </summary>
public class VideoErrors(VideoErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validator extensions.
    /// </summary>
    public VideoErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when a video is not found by its identifier.
    /// </summary>
    public NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("Video", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a video with the given slug already exists.
    /// </summary>
    public ConflictException SlugAlreadyExists(string slug)
    {
        return new ConflictException(i18n.SlugAlreadyExists(slug: slug));
    }

    /// <summary>
    /// Throws when a video title is required but not provided.
    /// </summary>
    public BadRequestException TitleRequired()
    {
        return new BadRequestException(i18n.TitleRequired());
    }

    /// <summary>
    /// Throws when a video slug is required but not provided.
    /// </summary>
    public BadRequestException SlugRequired()
    {
        return new BadRequestException(i18n.SlugRequired());
    }

    /// <summary>
    /// Throws when a video cannot be published because no YouTube URL has been attached.
    /// </summary>
    public BadRequestException CannotPublishWithoutYoutubeUrl()
    {
        return new BadRequestException(i18n.CannotPublishWithoutYoutubeUrl());
    }

    /// <summary>
    /// Throws when a hard delete is attempted on a published or approved video.
    /// </summary>
    public BadRequestException CannotDeletePublishedVideo()
    {
        return new BadRequestException(i18n.CannotDeletePublishedVideo());
    }

    /// <summary>
    /// Throws when a video is already pending payment.
    /// </summary>
    public ConflictException AlreadySubmitted()
    {
        return new ConflictException(i18n.AlreadySubmitted());
    }

    /// <summary>
    /// Throws when a video is already pending review.
    /// </summary>
    public ConflictException AlreadyPendingReview()
    {
        return new ConflictException(i18n.AlreadyPendingReview());
    }

    /// <summary>
    /// Throws when a video is already approved.
    /// </summary>
    public ConflictException AlreadyApproved()
    {
        return new ConflictException(i18n.AlreadyApproved());
    }

    /// <summary>
    /// Throws when a video is already published.
    /// </summary>
    public ConflictException AlreadyPublished()
    {
        return new ConflictException(i18n.AlreadyPublished());
    }

    /// <summary>
    /// Throws when a video is already rejected.
    /// </summary>
    public ConflictException AlreadyRejected()
    {
        return new ConflictException(i18n.AlreadyRejected());
    }

    /// <summary>
    /// Throws when a video is already archived.
    /// </summary>
    public ConflictException AlreadyArchived()
    {
        return new ConflictException(i18n.AlreadyArchived());
    }

    /// <summary>
    /// Throws when an invalid status transition is attempted.
    /// </summary>
    public BadRequestException InvalidStatusTransition(string from, string to)
    {
        return new BadRequestException(i18n.InvalidStatusTransition(from: from, to: to));
    }

    /// <summary>
    /// Throws when a YouTube URL is attached before the scheduled shooting date has passed.
    /// </summary>
    public BadRequestException CannotAttachYoutubeUrlBeforeShoot(DateTimeOffset shootingScheduledAt)
    {
        return new BadRequestException(
            i18n.CannotAttachYoutubeUrlBeforeShoot(shootingScheduledAt: shootingScheduledAt)
        );
    }

    /// <summary>
    /// Throws when a video carries no active promotion.
    /// </summary>
    public BadRequestException NotPromoted()
    {
        return new BadRequestException(i18n.NotPromoted());
    }
}
