using _116.Content.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUpdateLyricsValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Lyrics.Msg.Localizer);

        RuleFor(x => x.CategoryId).ValidLyricsCategoryId(i18n.Lyrics.Msg.CategoryIdRequired());

        RuleFor(x => x.SongTitle).ValidSongTitle(i18n.Lyrics.Msg);

        RuleFor(x => x.ArtistName).ValidArtistName(i18n.Lyrics.Msg);

        RuleFor(x => x.Slug).ValidLyricsSlug(i18n.Lyrics.Msg);

        RuleFor(x => x.LyricsText).ValidLyricsText(i18n.Lyrics.Msg);

        RuleFor(x => x.Language).ValidLyricsLanguage(i18n.Lyrics.Msg);

        When(x => x.CustomerId.HasValue, () => RuleFor(x => x.OrderItemId).ValidOrderItemId(i18n.ContentOrder.Msg));
        When(x => x.OrderItemId.HasValue, () => RuleFor(x => x.CustomerId).ValidCustomerId(i18n.Customer.Msg));
    }
}
