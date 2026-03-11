using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;

/// <summary>
/// Validator for the <see cref="UpdateCategoryCommand" /> ensuring proper category data format.
/// </summary>
public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    /// <summary>
    /// Configures validation rules for category update.
    /// </summary>
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Category ID");
        RuleFor(x => x.Name).ValidCategoryName();
        RuleFor(x => x.Slug).ValidCategorySlug();
        RuleFor(x => x.Description).ValidCategoryDescription();
    }
}
