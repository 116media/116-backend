using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo;

/// <summary>
/// Validator for the <see cref="AdminUpdateShortVideoCommand" /> ensuring proper update data.
/// </summary>
public class AdminUpdateShortVideoValidator : AbstractValidator<AdminUpdateShortVideoCommand>
{
    /// <summary>
    /// Configures validation rules for short video update.
    /// </summary>
    public AdminUpdateShortVideoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Short video ID");

        RuleFor(x => x.Title).ValidShortVideoTitle();

        RuleFor(x => x.Slug).ValidShortVideoSlug();

        When(
            x => x.VideoFile is not null,
            () =>
            {
                RuleFor(x => x.VideoFile).ValidShortVideoFile();
            }
        );
    }
}
