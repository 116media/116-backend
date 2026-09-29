using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsMetadata;

/// <summary>
/// Validator for the <see cref="AdminUpdateLyricsMetadataCommand" /> ensuring the release year
/// falls within a plausible range and each optional song-credit field respects its maximum length.
/// </summary>
public class AdminUpdateLyricsMetadataValidator : AbstractValidator<AdminUpdateLyricsMetadataCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdateLyricsMetadataValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    /// <param name="timeProvider">The clock the release-year upper boundary is computed from.</param>
    public AdminUpdateLyricsMetadataValidator(ContentI18n i18n, TimeProvider timeProvider)
    {
        RuleFor(x => x.ReleaseYear).ValidReleaseYear(timeProvider);
        RuleFor(x => x.Album).ValidAlbum(i18n.Lyrics.Msg);
        RuleFor(x => x.Label).ValidLabel(i18n.Lyrics.Msg);
        RuleFor(x => x.Songwriter).ValidSongwriter(i18n.Lyrics.Msg);
        RuleFor(x => x.Producer).ValidProducer(i18n.Lyrics.Msg);
    }
}
