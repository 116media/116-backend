using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Lyrics domain error factory providing simple, readable exception creation.
/// Usage: LyricsErrors.NotFound(id) or LyricsErrors.SlugAlreadyExists(slug)
/// </summary>
public class LyricsErrors(LyricsErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validator extensions.
    /// </summary>
    public LyricsErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when a lyrics record is not found by its identifier.
    /// </summary>
    public NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("Lyrics", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a song title is required but not provided.
    /// </summary>
    public BadRequestException SongTitleRequired()
    {
        return new BadRequestException(i18n.SongTitleRequired());
    }

    /// <summary>
    /// Throws when an artist name is required but not provided.
    /// </summary>
    public BadRequestException ArtistNameRequired()
    {
        return new BadRequestException(i18n.ArtistNameRequired());
    }

    /// <summary>
    /// Throws when lyrics text is required but not provided.
    /// </summary>
    public BadRequestException LyricsTextRequired()
    {
        return new BadRequestException(i18n.LyricsTextRequired());
    }

    /// <summary>
    /// Throws when a lyrics slug is required but not provided.
    /// </summary>
    public BadRequestException SlugRequired()
    {
        return new BadRequestException(i18n.SlugRequired());
    }

    /// <summary>
    /// Throws when a lyrics page with the given slug already exists.
    /// </summary>
    public ConflictException SlugAlreadyExists(string slug)
    {
        return new ConflictException(i18n.SlugAlreadyExists(slug: slug));
    }

    /// <summary>
    /// Throws when a lyrics page is already pending payment.
    /// </summary>
    public ConflictException AlreadySubmitted()
    {
        return new ConflictException(i18n.AlreadySubmitted());
    }

    /// <summary>
    /// Throws when a lyrics page is already pending review.
    /// </summary>
    public ConflictException AlreadyPendingReview()
    {
        return new ConflictException(i18n.AlreadyPendingReview());
    }

    /// <summary>
    /// Throws when a lyrics page is already approved.
    /// </summary>
    public ConflictException AlreadyApproved()
    {
        return new ConflictException(i18n.AlreadyApproved());
    }

    /// <summary>
    /// Throws when a lyrics page is already published.
    /// </summary>
    public ConflictException AlreadyPublished()
    {
        return new ConflictException(i18n.AlreadyPublished());
    }

    /// <summary>
    /// Throws when a lyrics page is already rejected.
    /// </summary>
    public ConflictException AlreadyRejected()
    {
        return new ConflictException(i18n.AlreadyRejected());
    }

    /// <summary>
    /// Throws when a lyrics page is already archived.
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
    /// Throws when a streaming link operation is attempted directly on a lyrics page that
    /// belongs to an album. A track that belongs to an album gets its streaming links through
    /// the album, not per-track.
    /// </summary>
    public ConflictException BelongsToAlbum()
    {
        return new ConflictException(i18n.BelongsToAlbum());
    }

    /// <summary>
    /// Throws when a lyrics page carries no active promotion.
    /// </summary>
    public BadRequestException NotPromoted()
    {
        return new BadRequestException(i18n.NotPromoted());
    }
}
