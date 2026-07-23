using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics;

/// <summary>
/// Validator for the <see cref="PublicSubmitLyricsCommand" /> ensuring the format of every
/// always-required field, plus the optional artist name and slug when they are present. The
/// artist-name-required-unless-verified-artist business rule is enforced in the handler, not
/// here, since it depends on a repository lookup this validator does not have access to.
/// </summary>
public class PublicSubmitLyricsValidator : AbstractValidator<PublicSubmitLyricsCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicSubmitLyricsValidator" /> with the
    /// specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public PublicSubmitLyricsValidator(ContentI18n i18n)
    {
        RuleFor(x => x.SongTitle).ValidSongTitle(i18n.Lyrics.Msg);

        RuleFor(x => x.LyricsText).ValidLyricsText(i18n.Lyrics.Msg);

        RuleFor(x => x.Language).ValidLyricsLanguage(i18n.Lyrics.Msg);

        RuleFor(x => x.Slug).ValidLyricsSlug(i18n.Lyrics.Msg, isRequired: false);

        When(
            x => !string.IsNullOrWhiteSpace(x.ArtistName),
            () => RuleFor(x => x.ArtistName).ValidArtistName(i18n.Lyrics.Msg)
        );
    }
}
