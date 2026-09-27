using _116.BuildingBlocks.Presentation.Extensions;
using _116.Content.Application.Shared.Errors.Facade;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeleteTag;

/// <summary>
/// Validator for the <see cref="AdminDeleteTagCommand" /> ensuring the tag ID is a valid GUID.
/// </summary>
public class AdminDeleteTagValidator : AbstractValidator<AdminDeleteTagCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminDeleteTagValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminDeleteTagValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Tag.Msg.Localizer);
    }
}
