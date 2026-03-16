using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateTag;

/// <summary>
/// Validator for the <see cref="AdminCreateTagCommand" /> ensuring proper tag data format.
/// </summary>
public class AdminCreateTagValidator : AbstractValidator<AdminCreateTagCommand>
{
    /// <summary>
    /// Configures validation rules for tag creation.
    /// </summary>
    public AdminCreateTagValidator()
    {
        RuleFor(x => x.Name).ValidTagName();
        RuleFor(x => x.Slug).ValidTagSlug();
    }
}
