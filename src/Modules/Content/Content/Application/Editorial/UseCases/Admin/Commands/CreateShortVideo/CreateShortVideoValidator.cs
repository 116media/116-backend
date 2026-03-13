using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo;

/// <summary>
/// Validator for the <see cref="CreateShortVideoCommand" /> ensuring proper short video creation data.
/// </summary>
public class CreateShortVideoValidator : AbstractValidator<CreateShortVideoCommand>
{
    /// <summary>
    /// Configures validation rules for short video creation.
    /// </summary>
    public CreateShortVideoValidator()
    {
        RuleFor(x => x.Title).ValidShortVideoTitle();

        RuleFor(x => x.VideoFile).ValidShortVideoFile();
    }
}
