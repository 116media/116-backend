using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SetArticleArtists;

/// <summary>
/// Validator for the <see cref="AdminSetArticleArtistsCommand" />. Empty is valid — it
/// untags everything — but the list itself must be present, bounded, and free of
/// duplicates.
/// </summary>
public class AdminSetArticleArtistsValidator : AbstractValidator<AdminSetArticleArtistsCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminSetArticleArtistsValidator" />.
    /// </summary>
    public AdminSetArticleArtistsValidator(ContentI18n i18n)
    {
        RuleFor(x => x.ArtistIds).ValidArticleArtistIds(i18n.Article.Msg);
    }
}
