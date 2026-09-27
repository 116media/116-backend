using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Video</c> domain.
/// Covers conflict situations and validation failures related to video operations.
/// </summary>
public class VideoErrorMessage(IStringLocalizer<VideoErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Gets an error message for when a video with the given slug already exists.
    /// </summary>
    /// <param name="slug">The video slug that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that a video with the specified slug already exists.
    /// </returns>
    public string SlugAlreadyExists(string slug)
    {
        return string.Format(localizer["SlugAlreadyExists"], slug);
    }

    /// <summary>
    /// Gets an error message for when a video title is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the video title is required.
    /// </returns>
    public string TitleRequired()
    {
        return localizer["TitleRequired"];
    }

    /// <summary>
    /// Gets an error message for when a video slug is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the video slug is required.
    /// </returns>
    public string SlugRequired()
    {
        return localizer["SlugRequired"];
    }

    /// <summary>
    /// Gets an error message for when a video cannot be published without an attached YouTube URL.
    /// </summary>
    /// <returns>
    /// An error message indicating that a YouTube video URL must be attached before the video can be published.
    /// </returns>
    public string CannotPublishWithoutYoutubeUrl()
    {
        return localizer["CannotPublishWithoutYoutubeUrl"];
    }

    /// <summary>
    /// Gets an error message for when a published or approved video is hard-deleted.
    /// </summary>
    /// <returns>
    /// An error message indicating that only videos in Draft or Rejected status can be permanently deleted.
    /// </returns>
    public string CannotDeletePublishedVideo()
    {
        return localizer["CannotDeletePublishedVideo"];
    }

    /// <summary>
    /// Gets an error message for when a video is already pending payment.
    /// </summary>
    public string AlreadySubmitted()
    {
        return localizer["AlreadySubmitted"];
    }

    /// <summary>
    /// Gets an error message for when a video is already pending review.
    /// </summary>
    public string AlreadyPendingReview()
    {
        return localizer["AlreadyPendingReview"];
    }

    /// <summary>
    /// Gets an error message for when a video is already approved.
    /// </summary>
    public string AlreadyApproved()
    {
        return localizer["AlreadyApproved"];
    }

    /// <summary>
    /// Gets an error message for when a video is already published.
    /// </summary>
    public string AlreadyPublished()
    {
        return localizer["AlreadyPublished"];
    }

    /// <summary>
    /// Gets an error message for when a video is already rejected.
    /// </summary>
    public string AlreadyRejected()
    {
        return localizer["AlreadyRejected"];
    }

    /// <summary>
    /// Gets an error message for when a video is already archived.
    /// </summary>
    public string AlreadyArchived()
    {
        return localizer["AlreadyArchived"];
    }

    /// <summary>
    /// Gets an error message for when an invalid status transition is attempted.
    /// </summary>
    /// <param name="from">The current status of the video.</param>
    /// <param name="to">The target status that the transition was attempted towards.</param>
    /// <returns>
    /// A formatted error message indicating that the transition from the current status to the target status is not allowed.
    /// </returns>
    public string InvalidStatusTransition(string from, string to)
    {
        return string.Format(localizer["InvalidStatusTransition"], from, to);
    }

    /// <summary>
    /// Gets an error message for when a YouTube URL is attached before the scheduled shooting date.
    /// </summary>
    /// <param name="shootingScheduledAt">
    /// The future date on which the shooting is scheduled.
    /// </param>
    /// <returns>
    /// An error message indicating that the YouTube URL cannot be attached until after the scheduled shooting date.
    /// </returns>
    public string CannotAttachYoutubeUrlBeforeShoot(DateTimeOffset shootingScheduledAt)
    {
        return string.Format(
            localizer["CannotAttachYoutubeUrlBeforeShoot"],
            shootingScheduledAt.ToString("yyyy-MM-dd")
        );
    }

    /// <summary>
    /// Gets an error message for when a video title exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string TitleTooLong(int max) => string.Format(localizer["TitleTooLong"], max);

    /// <summary>
    /// Gets an error message for when a video slug exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string SlugTooLong(int max) => string.Format(localizer["SlugTooLong"], max);

    /// <summary>
    /// Gets an error message for when a video slug has an invalid format.
    /// </summary>
    public string SlugInvalidFormat() => localizer["SlugInvalidFormat"];

    /// <summary>
    /// Gets an error message for when a video description is required.
    /// </summary>
    public string DescriptionRequired() => localizer["DescriptionRequired"];

    /// <summary>
    /// Gets an error message for when a YouTube video URL is required.
    /// </summary>
    public string YoutubeUrlRequired() => localizer["YoutubeUrlRequired"];

    /// <summary>
    /// Gets an error message for when a YouTube video URL exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string YoutubeUrlTooLong(int max) => string.Format(localizer["YoutubeUrlTooLong"], max);

    /// <summary>
    /// Gets an error message for when a YouTube URL is not a valid YouTube URL.
    /// </summary>
    public string YoutubeUrlInvalidFormat() => localizer["YoutubeUrlInvalidFormat"];

    /// <summary>
    /// Gets an error message for when the shooting scheduled date is not in the future.
    /// </summary>
    public string ShootingScheduledDateMustBeInFuture() => localizer["ShootingScheduledDateMustBeInFuture"];

    /// <summary>
    /// Gets an error message for when the popular-videos limit is outside the accepted range.
    /// </summary>
    /// <param name="min">The smallest accepted limit value.</param>
    /// <param name="max">The largest accepted limit value.</param>
    public string PopularLimitOutOfRange(int min, int max) =>
        string.Format(localizer["PopularLimitOutOfRange"], min, max);

    /// <summary>
    /// Gets an error message for when a video carries no active promotion.
    /// </summary>
    public string NotPromoted() => localizer["NotPromoted"];
}
