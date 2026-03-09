using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.DeactivateCategory;

/// <summary>
/// Validator for the <see cref="AdminDeactivateCategoryCommand" /> ensuring a valid category ID is provided.
/// </summary>
public class AdminDeactivateCategoryValidator : AbstractValidator<AdminDeactivateCategoryCommand>
{
    /// <summary>
    /// Configures validation rules for category deactivation.
    /// </summary>
    public AdminDeactivateCategoryValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Category ID");
    }
}
