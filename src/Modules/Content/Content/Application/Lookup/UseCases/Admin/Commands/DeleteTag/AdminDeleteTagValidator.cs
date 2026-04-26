using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeleteTag;

/// <summary>
/// Validator for the <see cref="AdminDeleteTagCommand" /> ensuring the tag ID is a valid GUID.
/// </summary>
public class AdminDeleteTagValidator : AbstractValidator<AdminDeleteTagCommand>
{
    /// <summary>
    /// Configures validation rules for tag deletion.
    /// </summary>
    public AdminDeleteTagValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Tag ID");
    }
}
