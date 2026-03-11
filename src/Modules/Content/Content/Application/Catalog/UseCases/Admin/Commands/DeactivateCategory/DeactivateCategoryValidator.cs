using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.DeactivateCategory;

/// <summary>
/// Validator for the <see cref="DeactivateCategoryCommand" /> ensuring a valid category ID is provided.
/// </summary>
public class DeactivateCategoryValidator : AbstractValidator<DeactivateCategoryCommand>
{
    /// <summary>
    /// Configures validation rules for category deactivation.
    /// </summary>
    public DeactivateCategoryValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Category ID");
    }
}
