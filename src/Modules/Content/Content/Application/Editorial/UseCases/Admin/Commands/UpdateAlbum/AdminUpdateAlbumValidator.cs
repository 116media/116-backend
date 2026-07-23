using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateAlbum;

/// <summary>
/// Validator for the <see cref="AdminUpdateAlbumCommand" /> ensuring proper album update data.
/// </summary>
public class AdminUpdateAlbumValidator : AbstractValidator<AdminUpdateAlbumCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdateAlbumValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUpdateAlbumValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Name).ValidAlbumName(i18n.Album.Msg);

        RuleFor(x => x.ReleaseYear).ValidReleaseYear();
    }
}
