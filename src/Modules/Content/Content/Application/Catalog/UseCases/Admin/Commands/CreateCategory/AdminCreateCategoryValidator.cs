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
    /// <param name="msg">Category validation error messages.</param>
    public AdminCreateCategoryValidator(CategoryErrorMessage msg)
    {
        RuleFor(x => x.ContentTypeId).IsValidGuid("Content type ID");
        RuleFor(x => x.Name).ValidCategoryName(msg);
        RuleFor(x => x.Slug).ValidCategorySlug(msg);
        RuleFor(x => x.Description).ValidCategoryDescription(msg);
    }
}
