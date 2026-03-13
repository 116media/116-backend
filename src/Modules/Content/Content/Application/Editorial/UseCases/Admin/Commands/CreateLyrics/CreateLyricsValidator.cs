using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;

/// <summary>
/// Validator for the <see cref="CreateLyricsCommand" /> ensuring proper lyrics creation data.
/// </summary>
public class CreateLyricsValidator : AbstractValidator<CreateLyricsCommand>
{
    /// <summary>
    /// Configures validation rules for lyrics creation.
    /// </summary>
    public CreateLyricsValidator()
    {
        RuleFor(x => x.SongTitle).ValidSongTitle();

        RuleFor(x => x.ArtistName).ValidArtistName();

        RuleFor(x => x.LyricsText).ValidLyricsText();

        RuleFor(x => x.Language).ValidLyricsLanguage();
    }
}
