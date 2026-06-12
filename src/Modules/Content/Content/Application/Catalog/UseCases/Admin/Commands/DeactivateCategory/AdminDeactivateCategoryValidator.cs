using _116.Content.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.DeactivateCategory;

/// <summary>
/// Validator for the <see cref="AdminDeactivateCategoryCommand" /> ensuring a valid category ID is provided.
/// </summary>
public class AdminDeactivateCategoryValidator : AbstractValidator<AdminDeactivateCategoryCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminDeactivateCategoryValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminDeactivateCategoryValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Category.Msg.Localizer);
    }
}
