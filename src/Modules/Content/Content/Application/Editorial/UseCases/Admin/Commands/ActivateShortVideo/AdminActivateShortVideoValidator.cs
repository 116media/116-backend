using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ActivateShortVideo;

/// <summary>
/// Validator for the <see cref="AdminActivateShortVideoCommand" /> ensuring a valid short video ID is provided.
/// </summary>
public class AdminActivateShortVideoValidator : AbstractValidator<AdminActivateShortVideoCommand>
{
    /// <summary>
    /// Configures validation rules for short video activation.
    /// </summary>
    public AdminActivateShortVideoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Short Video ID");
    }
}
