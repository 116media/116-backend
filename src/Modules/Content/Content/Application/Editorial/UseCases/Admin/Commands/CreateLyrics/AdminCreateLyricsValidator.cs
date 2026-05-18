using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;

/// <summary>
/// Validator for the <see cref="AdminCreateLyricsCommand" /> ensuring proper lyrics creation data.
/// </summary>
public class AdminCreateLyricsValidator : AbstractValidator<AdminCreateLyricsCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateLyricsValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="msg">Lyrics validation error messages.</param>
    public AdminCreateLyricsValidator(LyricsErrorMessage msg)
    {
        RuleFor(x => x.SongTitle).ValidSongTitle(msg);

        RuleFor(x => x.ArtistName).ValidArtistName(msg);

        RuleFor(x => x.LyricsText).ValidLyricsText(msg);

        RuleFor(x => x.Language).ValidLyricsLanguage(msg);
    }
}
