using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ActivateShortVideo;

/// <summary>
/// Validator for the <see cref="AdminActivateShortVideoCommand" /> ensuring a valid short video ID is provided.
/// </summary>
public class AdminActivateShortVideoValidator : AbstractValidator<AdminActivateShortVideoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminActivateShortVideoValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Short video validation error messages.</param>
    public AdminActivateShortVideoValidator(ShortVideoErrorMessage i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Localizer);
    }
}
