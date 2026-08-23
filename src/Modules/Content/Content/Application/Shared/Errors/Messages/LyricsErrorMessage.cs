using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Lyrics</c> domain.
/// Covers conflict situations and validation failures related to lyrics operations.
/// </summary>
public class LyricsErrorMessage(IStringLocalizer<LyricsErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Gets an error message for when a song title is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the song title is required.
    /// </returns>
    public string SongTitleRequired()
    {
        return localizer["SongTitleRequired"];
    }

    /// <summary>
    /// Gets an error message for when an artist name is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the artist name is required.
    /// </returns>
    public string ArtistNameRequired()
    {
        return localizer["ArtistNameRequired"];
    }

    /// <summary>
    /// Gets an error message for when lyrics text is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the lyrics text is required.
    /// </returns>
    public string LyricsTextRequired()
    {
        return localizer["LyricsTextRequired"];
    }

    /// <summary>
    /// Gets an error message for when a song title exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string SongTitleTooLong(int max) => string.Format(localizer["SongTitleTooLong"], max);

    /// <summary>
    /// Gets an error message for when an artist name exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string ArtistNameTooLong(int max) => string.Format(localizer["ArtistNameTooLong"], max);

    /// <summary>
    /// Gets an error message for when an album name exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string AlbumTooLong(int max) => string.Format(localizer["AlbumTooLong"], max);

    /// <summary>
    /// Gets an error message for when a record label name exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string LabelTooLong(int max) => string.Format(localizer["LabelTooLong"], max);

    /// <summary>
    /// Gets an error message for when a songwriter name exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string SongwriterTooLong(int max) => string.Format(localizer["SongwriterTooLong"], max);

    /// <summary>
    /// Gets an error message for when a producer name exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string ProducerTooLong(int max) => string.Format(localizer["ProducerTooLong"], max);

    /// <summary>
    /// Gets an error message for when a cover image file is required but not provided.
    /// </summary>
    public string FileRequired() => localizer["FileRequired"];

    /// <summary>
    /// Gets an error message for when lyrics language is required.
    /// </summary>
    public string LanguageRequired() => localizer["LanguageRequired"];

    /// <summary>
    /// Gets an error message for when lyrics language exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string LanguageTooLong(int max) => string.Format(localizer["LanguageTooLong"], max);

    /// <summary>
    /// Gets an error message for when a lyrics slug is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the lyrics slug is required.
    /// </returns>
    public string SlugRequired()
    {
        return localizer["SlugRequired"];
    }

    /// <summary>
    /// Gets an error message for when a lyrics page with the given slug already exists.
    /// </summary>
    /// <param name="slug">The lyrics slug that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that a lyrics page with the specified slug already exists.
    /// </returns>
    public string SlugAlreadyExists(string slug)
    {
        return string.Format(localizer["SlugAlreadyExists"], slug);
    }

    /// <summary>
    /// Gets an error message for when a lyrics slug exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string SlugTooLong(int max)
    {
        return string.Format(localizer["SlugTooLong"], max);
    }

    /// <summary>
    /// Gets an error message for when a lyrics slug has an invalid format.
    /// </summary>
    public string SlugInvalidFormat()
    {
        return localizer["SlugInvalidFormat"];
    }

    /// <summary>
    /// Gets an error message for when a lyrics page is already pending payment.
    /// </summary>
    public string AlreadySubmitted()
    {
        return localizer["AlreadySubmitted"];
    }

    /// <summary>
    /// Gets an error message for when a lyrics page is already pending review.
    /// </summary>
    public string AlreadyPendingReview()
    {
        return localizer["AlreadyPendingReview"];
    }

    /// <summary>
    /// Gets an error message for when a lyrics page is already approved.
    /// </summary>
    public string AlreadyApproved()
    {
        return localizer["AlreadyApproved"];
    }

    /// <summary>
    /// Gets an error message for when a lyrics page is already published.
    /// </summary>
    public string AlreadyPublished()
    {
        return localizer["AlreadyPublished"];
    }

    /// <summary>
    /// Gets an error message for when a lyrics page is already rejected.
    /// </summary>
    public string AlreadyRejected()
    {
        return localizer["AlreadyRejected"];
    }

    /// <summary>
    /// Gets an error message for when a lyrics page is already archived.
    /// </summary>
    public string AlreadyArchived()
    {
        return localizer["AlreadyArchived"];
    }

    /// <summary>
    /// Gets an error message for when an invalid status transition is attempted.
    /// </summary>
    /// <param name="from">The current status of the lyrics page.</param>
    /// <param name="to">The target status that the transition was attempted towards.</param>
    /// <returns>
    /// A formatted error message indicating that the transition from the current status to the target status is not allowed.
    /// </returns>
    public string InvalidStatusTransition(string from, string to)
    {
        return string.Format(localizer["InvalidStatusTransition"], from, to);
    }

    /// <summary>
    /// Gets an error message for when a category ID is required.
    /// </summary>
    public string CategoryIdRequired() => localizer["CategoryIdRequired"];

    /// <summary>
    /// Gets an error message for when a lyrics rejection reason is required.
    /// </summary>
    public string RejectionReasonRequired() => localizer["RejectionReasonRequired"];

    /// <summary>
    /// Gets an error message for when a lyrics rejection reason exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string RejectionReasonTooLong(int max) => string.Format(localizer["RejectionReasonTooLong"], max);

    /// <summary>
    /// Gets an error message for when a streaming link operation is attempted directly on a
    /// lyrics page that belongs to an album.
    /// </summary>
    public string BelongsToAlbum() => localizer["BelongsToAlbum"];

    /// <summary>
    /// Gets an error message for when a lyrics page carries no active promotion.
    /// </summary>
    public string NotPromoted() => localizer["NotPromoted"];
}
