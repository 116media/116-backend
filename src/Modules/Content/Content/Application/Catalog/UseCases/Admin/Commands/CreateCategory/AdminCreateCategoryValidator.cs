using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory;

/// <summary>
/// Validator for the <see cref="AdminCreateCategoryCommand" /> ensuring proper category data format.
/// </summary>
public class AdminCreateCategoryValidator : AbstractValidator<AdminCreateCategoryCommand>
{
    /// <summary>
    /// Configures validation rules for category creation.
    /// </summary>
    public AdminCreateCategoryValidator()
    {
        RuleFor(x => x.ContentTypeId).IsValidGuid("Content type ID");
        RuleFor(x => x.Name).ValidCategoryName();
        RuleFor(x => x.Slug).ValidCategorySlug();
        RuleFor(x => x.Description).ValidCategoryDescription();
    }
}
