using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCategoryById;

/// <summary>
/// Validator for the <see cref="AdminGetCategoryByIdQuery" /> ensuring a valid category ID is provided.
/// </summary>
public class AdminGetCategoryByIdValidator : AbstractValidator<AdminGetCategoryByIdQuery>
{
    /// <summary>
    /// Configures validation rules for category retrieval by ID.
    /// </summary>
    public AdminGetCategoryByIdValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Category ID");
    }
}
