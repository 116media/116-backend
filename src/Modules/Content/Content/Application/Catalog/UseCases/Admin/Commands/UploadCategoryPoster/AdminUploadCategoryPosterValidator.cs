using _116.Content.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UploadCategoryPoster;

/// <summary>
/// Validator for the <see cref="AdminUploadCategoryPosterCommand" /> ensuring required fields are provided.
/// </summary>
public class AdminUploadCategoryPosterValidator : AbstractValidator<AdminUploadCategoryPosterCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUploadCategoryPosterValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUploadCategoryPosterValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Category.Msg.Localizer);

        RuleFor(x => x.File).NotNull().WithMessage(i18n.Category.Msg.FileRequired());
    }
}
