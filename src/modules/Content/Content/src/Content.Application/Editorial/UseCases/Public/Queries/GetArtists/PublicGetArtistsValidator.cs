using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtists;

/// <summary>
/// Validator for the <see cref="PublicGetArtistsQuery" />. The letter and search filters
/// are mutually exclusive; picking one silently would let a broken client render
/// plausible-but-wrong results forever. A search shorter than two characters scans the
/// whole table to return a page nobody can use.
/// </summary>
public class PublicGetArtistsValidator : AbstractValidator<PublicGetArtistsQuery>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicGetArtistsValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public PublicGetArtistsValidator(ContentI18n i18n)
    {
        RuleFor(x => x).ExclusiveArtistDirectoryFilters(x => x.Letter, x => x.Search, i18n.Artist.Msg);

        RuleFor(x => x.Letter).ValidArtistLetterBucket(i18n.Artist.Msg).When(x => !string.IsNullOrWhiteSpace(x.Letter));

        RuleFor(x => x.Search).ValidArtistSearch(i18n.Artist.Msg).When(x => !string.IsNullOrWhiteSpace(x.Search));
    }
}
