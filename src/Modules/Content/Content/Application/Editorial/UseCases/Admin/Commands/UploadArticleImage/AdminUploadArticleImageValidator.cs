using _116.Content.Application.Shared.Errors.Messages;
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
    /// <param name="msg">Article validation error messages.</param>
    public AdminUploadArticleImageValidator(ArticleErrorMessage msg)
    {
        RuleFor(x => x.ArticleId).IsValidGuid("Article ID");

        RuleFor(x => x.File).ValidArticleImageFile(msg);
    }
}
