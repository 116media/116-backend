using _116.Content.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeactivateShortVideo;

/// <summary>
/// Validator for the <see cref="AdminDeactivateShortVideoCommand" /> ensuring a valid short video ID is provided.
/// </summary>
public class AdminDeactivateShortVideoValidator : AbstractValidator<AdminDeactivateShortVideoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminDeactivateShortVideoValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminDeactivateShortVideoValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.ShortVideo.Msg.Localizer);
    }
}
