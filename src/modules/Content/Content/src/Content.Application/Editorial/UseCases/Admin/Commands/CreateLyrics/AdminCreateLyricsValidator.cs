using _116.Content.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminCreateLyricsValidator(ContentI18n i18n)
    {
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
