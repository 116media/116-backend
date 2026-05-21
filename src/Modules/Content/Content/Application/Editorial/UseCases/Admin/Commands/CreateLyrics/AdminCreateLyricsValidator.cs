using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
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
        RuleFor(x => x.SongTitle)
            .ValidSongTitle(
                songTitleRequired: msg.SongTitleRequired(),
                songTitleTooLong: msg.SongTitleTooLong(ContentConstants.MaxSongTitleLength)
            );

        RuleFor(x => x.ArtistName)
            .ValidArtistName(
                artistNameRequired: msg.ArtistNameRequired(),
                artistNameTooLong: msg.ArtistNameTooLong(ContentConstants.MaxArtistNameLength)
            );

        RuleFor(x => x.LyricsText).ValidLyricsText(lyricsTextRequired: msg.LyricsTextRequired());

        RuleFor(x => x.Language)
            .ValidLyricsLanguage(
                languageRequired: msg.LanguageRequired(),
                languageTooLong: msg.LanguageTooLong(ContentConstants.MaxLyricsLanguageLength)
            );
    }
}
