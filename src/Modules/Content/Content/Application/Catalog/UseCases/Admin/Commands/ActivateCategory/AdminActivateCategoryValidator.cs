using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivateCategory;

/// <summary>
/// Validator for the <see cref="AdminActivateCategoryCommand" /> ensuring a valid category ID is provided.
/// </summary>
public class AdminActivateCategoryValidator : AbstractValidator<AdminActivateCategoryCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminActivateCategoryValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Category validation error messages.</param>
    public AdminActivateCategoryValidator(CategoryErrorMessage i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Localizer);
    }
}
