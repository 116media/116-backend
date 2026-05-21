using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
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
    /// <param name="msg">Category validation error messages.</param>
    public AdminCreateCategoryValidator(CategoryErrorMessage msg)
    {
        RuleFor(x => x.ContentTypeId).IsValidGuid("Content type ID");
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
