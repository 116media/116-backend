using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateAlbum;

/// <summary>
/// Validator for the <see cref="AdminCreateAlbumCommand" /> ensuring proper album creation data.
/// </summary>
public class AdminCreateAlbumValidator : AbstractValidator<AdminCreateAlbumCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateAlbumValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminCreateAlbumValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Name).ValidAlbumName(i18n.Album.Msg);

        RuleFor(x => x.ReleaseYear).ValidReleaseYear();

        RuleFor(x => x.ReleaseType).IsInEnum();
    }
}
