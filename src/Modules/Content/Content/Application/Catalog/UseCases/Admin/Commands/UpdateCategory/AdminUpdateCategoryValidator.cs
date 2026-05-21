using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
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
    /// <param name="msg">Category validation error messages.</param>
    public AdminUpdateCategoryValidator(CategoryErrorMessage msg)
    {
        RuleFor(x => x.Id).IsValidGuid("Category ID");
        RuleFor(x => x.Name)
            .ValidCategoryName(
                nameRequired: msg.NameRequired(),
                nameTooLong: msg.NameTooLong(ContentConstants.MaxCategoryNameLength)
            );
        RuleFor(x => x.Slug)
            .ValidCategorySlug(
                slugRequired: msg.SlugRequired(),
                slugTooLong: msg.SlugTooLong(ContentConstants.MaxCategorySlugLength),
                slugInvalidFormat: msg.SlugInvalidFormat()
            );
        RuleFor(x => x.Description)
            .ValidCategoryDescription(
                descriptionRequired: msg.DescriptionRequired(),
                descriptionTooLong: msg.DescriptionTooLong(ContentConstants.MaxCategoryDescriptionLength)
            );
    }
}
