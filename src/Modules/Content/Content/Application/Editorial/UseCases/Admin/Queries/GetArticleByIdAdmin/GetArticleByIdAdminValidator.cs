using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleByIdAdmin;

/// <summary>
/// Validator for the <see cref="GetArticleByIdAdminQuery" /> ensuring a valid article ID is provided.
/// </summary>
public class GetArticleByIdAdminValidator : AbstractValidator<GetArticleByIdAdminQuery>
{
    /// <summary>
    /// Configures validation rules for retrieving an article by ID.
    /// </summary>
    public GetArticleByIdAdminValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Article ID");
    }
}
