using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveVideo;

/// <summary>
/// Validator for the <see cref="AdminArchiveVideoCommand" /> ensuring a valid video ID is provided.
/// </summary>
public class AdminArchiveVideoValidator : AbstractValidator<AdminArchiveVideoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminArchiveVideoValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Video validation error messages.</param>
    public AdminArchiveVideoValidator(VideoErrorMessage i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Localizer);
    }
}
