using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage;

/// <summary>
/// Validator for the <see cref="UploadArticleImageCommand" /> ensuring required fields are provided.
/// </summary>
public class UploadArticleImageValidator : AbstractValidator<UploadArticleImageCommand>
{
    /// <summary>
    /// Configures validation rules for article image upload.
    /// </summary>
    public UploadArticleImageValidator()
    {
        RuleFor(x => x.ArticleId).IsValidGuid("Article ID");

        RuleFor(x => x.File).ValidArticleImageFile();
    }
}
