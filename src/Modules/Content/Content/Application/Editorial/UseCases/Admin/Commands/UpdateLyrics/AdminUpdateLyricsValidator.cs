using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;

/// <summary>
/// Validator for the <see cref="AdminUpdateLyricsCommand" /> ensuring proper lyrics update data.
/// </summary>
public class AdminUpdateLyricsValidator : AbstractValidator<AdminUpdateLyricsCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdateLyricsValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Lyrics validation error messages.</param>
    public AdminUpdateLyricsValidator(LyricsErrorMessage i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Localizer);

        RuleFor(x => x.SongTitle).ValidSongTitle(i18n);

        RuleFor(x => x.ArtistName).ValidArtistName(i18n);

        RuleFor(x => x.LyricsText).ValidLyricsText(i18n);

        RuleFor(x => x.Language).ValidLyricsLanguage(i18n);
    }
}
