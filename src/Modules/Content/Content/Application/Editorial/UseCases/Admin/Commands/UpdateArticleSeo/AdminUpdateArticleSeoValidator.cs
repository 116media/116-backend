using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleSeo;

/// <summary>
/// Validator for the <see cref="AdminUpdateArticleSeoCommand" /> ensuring a valid article ID is provided
/// and that SEO fields respect their length constraints when supplied.
/// </summary>
public class AdminUpdateArticleSeoValidator : AbstractValidator<AdminUpdateArticleSeoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdateArticleSeoValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="msg">Article validation error messages.</param>
    public AdminUpdateArticleSeoValidator(ArticleErrorMessage msg)
    {
        RuleFor(x => x.Id).IsValidGuid("Article ID");

        When(x => x.MetaTitle is not null, () => RuleFor(x => x.MetaTitle).ValidMetaTitle(msg));
        When(x => x.MetaDescription is not null, () => RuleFor(x => x.MetaDescription).ValidMetaDescription(msg));
    }
}
