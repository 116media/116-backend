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
    /// <param name="i18n">Lyrics validation error messages.</param>
    public AdminCreateLyricsValidator(LyricsErrorMessage i18n)
    {
        RuleFor(x => x.SongTitle).ValidSongTitle(i18n);

        RuleFor(x => x.ArtistName).ValidArtistName(i18n);

        RuleFor(x => x.LyricsText).ValidLyricsText(i18n);

        RuleFor(x => x.Language).ValidLyricsLanguage(i18n);
    }
}
