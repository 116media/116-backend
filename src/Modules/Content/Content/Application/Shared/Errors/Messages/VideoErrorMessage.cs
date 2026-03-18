namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Video</c> domain.
/// Covers conflict situations and validation failures related to video operations.
/// </summary>
public static class VideoErrorMessage
{
    /// <summary>
    /// Gets an error message for when a video with the given slug already exists.
    /// </summary>
    /// <param name="slug">The video slug that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that a video with the specified slug already exists.
    /// </returns>
    public static string SlugAlreadyExists(string slug)
    {
        return $"Video with slug '{slug}' already exists";
    }

    /// <summary>
    /// Gets an error message for when a video title is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the video title is required.
    /// </returns>
    public static string TitleRequired()
    {
        return "Video title is required";
    }

    /// <summary>
    /// Gets an error message for when a video slug is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the video slug is required.
    /// </returns>
    public static string SlugRequired()
    {
        return "Video slug is required";
    }

    /// <summary>
    /// Gets an error message for when a video cannot be published without an attached YouTube ID.
    /// </summary>
    /// <returns>
    /// An error message indicating that a YouTube video ID must be attached before the video can be published.
    /// </returns>
    public static string CannotPublishWithoutYoutubeId()
    {
        return "A YouTube video ID must be attached before this video can be published";
    }

    /// <summary>
    /// Gets an error message for when a published or approved video is hard-deleted.
    /// </summary>
    /// <returns>
    /// An error message indicating that only videos in Draft or Rejected status can be permanently deleted.
    /// </returns>
    public static string CannotDeletePublishedVideo()
    {
        return "Only videos in Draft or Rejected status can be permanently deleted. Archive published videos instead.";
    }

    /// <summary>
    /// Gets an error message for when an invalid status transition is attempted.
    /// </summary>
    /// <param name="from">The current status of the video.</param>
    /// <param name="to">The target status that the transition was attempted towards.</param>
    /// <returns>
    /// A formatted error message indicating that the transition from the current status to the target status is not allowed.
    /// </returns>
    /// <summary>
    /// Gets an error message for when a video is already pending payment.
    /// </summary>
    public static string AlreadySubmitted()
    {
        return "Video is already pending payment";
    }

    /// <summary>
    /// Gets an error message for when a video is already pending review.
    /// </summary>
    public static string AlreadyPendingReview()
    {
        return "Video is already pending review";
    }

    /// <summary>
    /// Gets an error message for when a video is already approved.
    /// </summary>
    public static string AlreadyApproved()
    {
        return "Video is already approved";
    }

    /// <summary>
    /// Gets an error message for when a video is already published.
    /// </summary>
    public static string AlreadyPublished()
    {
        return "Video is already published";
    }

    /// <summary>
    /// Gets an error message for when a video is already rejected.
    /// </summary>
    public static string AlreadyRejected()
    {
        return "Video is already rejected";
    }

    /// <summary>
    /// Gets an error message for when a video is already archived.
    /// </summary>
    public static string AlreadyArchived()
    {
        return "Video is already archived";
    }

    /// <summary>
    /// Gets an error message for when an invalid status transition is attempted.
    /// </summary>
    /// <param name="from">The current status of the video.</param>
    /// <param name="to">The target status that the transition was attempted towards.</param>
    /// <returns>
    /// A formatted error message indicating that the transition from the current status to the target status is not allowed.
    /// </returns>
    public static string InvalidStatusTransition(string from, string to)
    {
        return $"Cannot transition video from '{from}' to '{to}'";
    }
}
