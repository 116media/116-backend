using _116.Content.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminArchiveVideoValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Video.Msg.Localizer);
    }
}
