using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateTag;

/// <summary>
/// Validator for the <see cref="AdminCreateTagCommand" /> ensuring proper tag data format.
/// </summary>
public class AdminCreateTagValidator : AbstractValidator<AdminCreateTagCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateTagValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Tag validation error messages.</param>
    public AdminCreateTagValidator(TagErrorMessage i18n)
    {
        RuleFor(x => x.Name).ValidTagName(i18n);
        RuleFor(x => x.Slug).ValidTagSlug(i18n);
    }
}
