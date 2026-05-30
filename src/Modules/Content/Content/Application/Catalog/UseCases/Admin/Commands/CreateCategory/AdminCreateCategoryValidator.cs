using _116.Content.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminCreateCategoryValidator(ContentI18n i18n)
    {
        RuleFor(x => x.ContentTypeId).IsValidGuid(i18n.ContentType.Msg.Localizer);
        RuleFor(x => x.Name).ValidCategoryName(i18n.Category.Msg);
        RuleFor(x => x.Slug).ValidCategorySlug(i18n.Category.Msg);
        RuleFor(x => x.Description).ValidCategoryDescription(i18n.Category.Msg);
    }
}
