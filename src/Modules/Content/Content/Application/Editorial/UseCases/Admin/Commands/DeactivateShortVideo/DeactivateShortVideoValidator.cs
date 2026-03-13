using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeactivateShortVideo;

/// <summary>
/// Validator for the <see cref="DeactivateShortVideoCommand" /> ensuring a valid short video ID is provided.
/// </summary>
public class DeactivateShortVideoValidator : AbstractValidator<DeactivateShortVideoCommand>
{
    /// <summary>
    /// Configures validation rules for short video deactivation.
    /// </summary>
    public DeactivateShortVideoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Short Video ID");
    }
}
