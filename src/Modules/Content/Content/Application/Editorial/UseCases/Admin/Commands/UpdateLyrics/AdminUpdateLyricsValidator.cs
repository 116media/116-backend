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
    /// <param name="msg">Lyrics validation error messages.</param>
    public AdminUpdateLyricsValidator(LyricsErrorMessage msg)
    {
        RuleFor(x => x.Id).IsValidGuid("Lyrics ID");

        RuleFor(x => x.SongTitle).ValidSongTitle(msg);

        RuleFor(x => x.ArtistName).ValidArtistName(msg);

        RuleFor(x => x.LyricsText).ValidLyricsText(msg);

        RuleFor(x => x.Language).ValidLyricsLanguage(msg);
    }
}
