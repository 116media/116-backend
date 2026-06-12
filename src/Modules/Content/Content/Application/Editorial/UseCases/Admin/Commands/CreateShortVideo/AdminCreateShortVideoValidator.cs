using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo;

/// <summary>
/// Validator for the <see cref="AdminCreateShortVideoCommand" /> ensuring proper short video creation data.
/// </summary>
public class AdminCreateShortVideoValidator : AbstractValidator<AdminCreateShortVideoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateShortVideoValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Short video validation error messages.</param>
    public AdminCreateShortVideoValidator(ShortVideoErrorMessage i18n)
    {
        RuleFor(x => x.Title).ValidShortVideoTitle(i18n);

        RuleFor(x => x.Slug).ValidShortVideoSlug(i18n);

        RuleFor(x => x.VideoFile).ValidShortVideoFile(i18n);
    }
}
