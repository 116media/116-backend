using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCategoryById;

/// <summary>
/// Validator for the <see cref="GetCategoryByIdQuery" /> ensuring a valid category ID is provided.
/// </summary>
public class GetCategoryByIdValidator : AbstractValidator<GetCategoryByIdQuery>
{
    /// <summary>
    /// Configures validation rules for category retrieval by ID.
    /// </summary>
    public GetCategoryByIdValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Category ID");
    }
}
