using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Lyrics domain error factory providing simple, readable exception creation.
/// Usage: LyricsErrors.NotFound(id) or LyricsErrors.AlreadyExists(songTitle, artistName)
/// </summary>
public class LyricsErrors(LyricsErrorMessage i18n)
{
    /// <summary>
    /// Throws when a lyrics record is not found by its identifier.
    /// </summary>
    public NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("Lyrics", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when lyrics for the given song and artist already exist.
    /// </summary>
    public ConflictException AlreadyExists(string songTitle, string artistName)
    {
        return new ConflictException(i18n.AlreadyExists(songTitle: songTitle, artistName: artistName));
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
}
