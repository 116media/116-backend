using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;

/// <summary>
/// Validator for the <see cref="AdminUpdateCategoryCommand" /> ensuring proper category data format.
/// </summary>
public class AdminUpdateCategoryValidator : AbstractValidator<AdminUpdateCategoryCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdateCategoryValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Category validation error messages.</param>
    public AdminUpdateCategoryValidator(CategoryErrorMessage i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Localizer);
        RuleFor(x => x.Name).ValidCategoryName(i18n);
        RuleFor(x => x.Slug).ValidCategorySlug(i18n);
        RuleFor(x => x.Description).ValidCategoryDescription(i18n);
    }
}
