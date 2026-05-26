using _116.Content.Application.Shared.Errors.Messages;
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
    /// Initializes a new instance of <see cref="AdminCreateCategoryValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Category validation error messages.</param>
    /// <param name="contentTypeMsg">Content type validation error messages.</param>
    public AdminCreateCategoryValidator(CategoryErrorMessage i18n, ContentTypeErrorMessage contentTypeMsg)
    {
        RuleFor(x => x.ContentTypeId).IsValidGuid(contentTypeMsg.Localizer);
        RuleFor(x => x.Name).ValidCategoryName(i18n);
        RuleFor(x => x.Slug).ValidCategorySlug(i18n);
        RuleFor(x => x.Description).ValidCategoryDescription(i18n);
    }
}
