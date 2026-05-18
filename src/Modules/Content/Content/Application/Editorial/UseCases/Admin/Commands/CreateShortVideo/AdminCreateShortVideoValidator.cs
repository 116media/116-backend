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
    /// <param name="msg">Short video validation error messages.</param>
    public AdminCreateShortVideoValidator(ShortVideoErrorMessage msg)
    {
        RuleFor(x => x.Title).ValidShortVideoTitle(msg);

        RuleFor(x => x.Slug).ValidShortVideoSlug(msg);

        RuleFor(x => x.VideoFile).ValidShortVideoFile(msg);
    }
}
