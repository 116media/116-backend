using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleById;

/// <summary>
/// Validator for the <see cref="AdminGetArticleByIdQuery" /> ensuring a valid article ID is provided.
/// </summary>
public class AdminGetArticleByIdValidator : AbstractValidator<AdminGetArticleByIdQuery>
{
    /// <summary>
    /// Configures validation rules for retrieving an article by ID.
    /// </summary>
    public AdminGetArticleByIdValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Article ID");
    }
}
