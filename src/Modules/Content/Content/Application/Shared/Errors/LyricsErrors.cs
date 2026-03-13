using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Lyrics domain error factory providing simple, readable exception creation.
/// Usage: LyricsErrors.NotFound(id) or LyricsErrors.AlreadyExists(songTitle, artistName)
/// </summary>
public static class LyricsErrors
{
    /// <summary>
    /// Throws when a lyrics record is not found by its identifier.
    /// </summary>
    public static NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("Lyrics", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when lyrics for the given song and artist already exist.
    /// </summary>
    public static ConflictException AlreadyExists(string songTitle, string artistName)
    {
        return new ConflictException(LyricsErrorMessage.AlreadyExists(songTitle: songTitle, artistName: artistName));
    }

    /// <summary>
    /// Throws when a song title is required but not provided.
    /// </summary>
    public static BadRequestException SongTitleRequired()
    {
        return new BadRequestException(LyricsErrorMessage.SongTitleRequired());
    }

    /// <summary>
    /// Throws when an artist name is required but not provided.
    /// </summary>
    public static BadRequestException ArtistNameRequired()
    {
        return new BadRequestException(LyricsErrorMessage.ArtistNameRequired());
    }

    /// <summary>
    /// Throws when lyrics text is required but not provided.
    /// </summary>
    public static BadRequestException LyricsTextRequired()
    {
        return new BadRequestException(LyricsErrorMessage.LyricsTextRequired());
    }
}
