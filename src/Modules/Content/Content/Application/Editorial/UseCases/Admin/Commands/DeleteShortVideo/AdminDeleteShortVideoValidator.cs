using _116.Content.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteShortVideo;

/// <summary>
/// Validator for the <see cref="AdminDeleteShortVideoCommand" /> ensuring a valid short video ID is provided.
/// </summary>
public class AdminDeleteShortVideoValidator : AbstractValidator<AdminDeleteShortVideoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminDeleteShortVideoValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminDeleteShortVideoValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.ShortVideo.Msg.Localizer);
    }
}
