using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteVideo;

/// <summary>
/// Validator for the <see cref="AdminDeleteVideoCommand" /> ensuring a valid video ID is provided.
/// </summary>
public class AdminDeleteVideoValidator : AbstractValidator<AdminDeleteVideoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminDeleteVideoValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Video validation error messages.</param>
    public AdminDeleteVideoValidator(VideoErrorMessage i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Localizer);
    }
}
