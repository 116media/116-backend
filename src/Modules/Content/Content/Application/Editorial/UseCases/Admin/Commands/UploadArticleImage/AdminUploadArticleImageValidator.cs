using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage;

/// <summary>
/// Validator for the <see cref="AdminUploadArticleImageCommand" /> ensuring required fields are provided.
/// </summary>
public class AdminUploadArticleImageValidator : AbstractValidator<AdminUploadArticleImageCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUploadArticleImageValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUploadArticleImageValidator(ContentI18n i18n)
    {
        RuleFor(x => x.ArticleId).IsValidGuid(i18n.Article.Msg.Localizer);

        RuleFor(x => x.File).ValidArticleImageFile(i18n.Article.Msg);
    }
}
