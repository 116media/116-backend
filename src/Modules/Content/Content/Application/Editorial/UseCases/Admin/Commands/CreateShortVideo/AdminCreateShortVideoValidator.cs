using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo;

/// <summary>
/// Validator for the <see cref="AdminCreateShortVideoCommand" /> ensuring proper short video creation data.
/// </summary>
public class AdminCreateShortVideoValidator : AbstractValidator<AdminCreateShortVideoCommand>
{
    /// <summary>
    /// Configures validation rules for short video creation.
    /// </summary>
    public AdminCreateShortVideoValidator()
    {
        RuleFor(x => x.Title).ValidShortVideoTitle();

        RuleFor(x => x.Slug).ValidShortVideoSlug();

        RuleFor(x => x.VideoFile).ValidShortVideoFile();
    }
}
