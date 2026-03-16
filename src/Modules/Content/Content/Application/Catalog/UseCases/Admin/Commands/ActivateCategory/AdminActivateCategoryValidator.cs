using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivateCategory;

/// <summary>
/// Validator for the <see cref="AdminActivateCategoryCommand" /> ensuring a valid category ID is provided.
/// </summary>
public class AdminActivateCategoryValidator : AbstractValidator<AdminActivateCategoryCommand>
{
    /// <summary>
    /// Configures validation rules for category activation.
    /// </summary>
    public AdminActivateCategoryValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Category ID");
    }
}
