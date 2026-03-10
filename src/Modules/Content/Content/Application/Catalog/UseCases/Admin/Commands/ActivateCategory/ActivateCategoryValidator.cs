using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivateCategory;

/// <summary>
/// Validator for the <see cref="ActivateCategoryCommand" /> ensuring a valid category ID is provided.
/// </summary>
public class ActivateCategoryValidator : AbstractValidator<ActivateCategoryCommand>
{
    /// <summary>
    /// Configures validation rules for category activation.
    /// </summary>
    public ActivateCategoryValidator()
    {
        RuleFor(x => x.Id).ValidCategoryId();
    }
}
