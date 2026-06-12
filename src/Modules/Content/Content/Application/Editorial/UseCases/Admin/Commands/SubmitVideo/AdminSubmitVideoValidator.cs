using _116.Content.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitVideo;

/// <summary>
/// Validator for the <see cref="AdminSubmitVideoCommand" /> ensuring a valid video ID is provided.
/// </summary>
public class AdminSubmitVideoValidator : AbstractValidator<AdminSubmitVideoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminSubmitVideoValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminSubmitVideoValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Video.Msg.Localizer);
    }
}
