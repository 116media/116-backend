using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
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
    /// <param name="i18n">Tag validation error messages.</param>
    public AdminDeleteTagValidator(TagErrorMessage i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Localizer);
    }
}
