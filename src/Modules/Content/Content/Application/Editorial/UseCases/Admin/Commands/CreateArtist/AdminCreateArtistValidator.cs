using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist;

/// <summary>
/// Validator for the <see cref="AdminCreateArtistCommand" /> ensuring proper artist profile creation data.
/// </summary>
public class AdminCreateArtistValidator : AbstractValidator<AdminCreateArtistCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateArtistValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminCreateArtistValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Name).ValidArtistName(i18n.Artist.Msg);

        RuleFor(x => x.Slug).ValidArtistSlug(i18n.Artist.Msg);

        RuleFor(x => x.RealName).ValidArtistRealName();

        RuleFor(x => x.Aliases).ValidArtistAliases(i18n.Artist.Msg);

        RuleFor(x => x.Birthdate).ValidArtistBirthdate(i18n.Artist.Msg);

        RuleFor(x => x.Hometown).ValidArtistHometown();
    }
}
