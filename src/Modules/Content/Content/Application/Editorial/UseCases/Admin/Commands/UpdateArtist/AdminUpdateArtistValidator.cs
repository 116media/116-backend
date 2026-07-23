using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArtist;

/// <summary>
/// Validator for the <see cref="AdminUpdateArtistCommand" /> ensuring proper artist profile update data.
/// </summary>
public class AdminUpdateArtistValidator : AbstractValidator<AdminUpdateArtistCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdateArtistValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUpdateArtistValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Name).ValidArtistName(i18n.Artist.Msg);
    }
}
