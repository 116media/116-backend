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
    /// Configures validation rules for article image upload.
    /// </summary>
    public AdminUploadArticleImageValidator()
    {
        RuleFor(x => x.ArticleId).IsValidGuid("Article ID");

        RuleFor(x => x.File).ValidArticleImageFile();
    }
}
