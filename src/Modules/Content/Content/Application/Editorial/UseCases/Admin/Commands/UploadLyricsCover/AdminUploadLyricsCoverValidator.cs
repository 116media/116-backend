using _116.Content.Application.Shared.Errors.Facade;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadLyricsCover;

/// <summary>
/// Validator for the <see cref="AdminUploadLyricsCoverCommand" /> ensuring a cover image file is provided.
/// </summary>
public class AdminUploadLyricsCoverValidator : AbstractValidator<AdminUploadLyricsCoverCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUploadLyricsCoverValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUploadLyricsCoverValidator(ContentI18n i18n)
    {
        RuleFor(x => x.File).NotNull().WithMessage(i18n.Lyrics.Msg.FileRequired());
    }
}
