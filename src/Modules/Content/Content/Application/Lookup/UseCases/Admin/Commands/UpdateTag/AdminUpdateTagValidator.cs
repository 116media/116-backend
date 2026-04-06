using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag;

/// <summary>
/// Validator for the <see cref="AdminUpdateTagCommand" /> ensuring proper tag data format.
/// </summary>
public class AdminUpdateTagValidator : AbstractValidator<AdminUpdateTagCommand>
{
    /// <summary>
    /// Configures validation rules for tag update.
    /// </summary>
    public AdminUpdateTagValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Tag ID");
        RuleFor(x => x.Name).ValidTagName();
        RuleFor(x => x.Slug).ValidTagSlug();
    }
}
