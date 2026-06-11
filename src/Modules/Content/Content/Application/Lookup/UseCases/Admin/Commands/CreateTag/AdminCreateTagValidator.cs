using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
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
    /// <param name="msg">Tag validation error messages.</param>
    public AdminCreateTagValidator(TagErrorMessage msg)
    {
        RuleFor(x => x.Name)
            .ValidTagName(
                nameRequired: msg.NameRequired(),
                nameTooLong: msg.NameTooLong(ContentConstants.MaxTagNameLength)
            );
        RuleFor(x => x.Slug)
            .ValidTagSlug(
                slugRequired: msg.SlugRequired(),
                slugTooLong: msg.SlugTooLong(ContentConstants.MaxTagSlugLength),
                slugInvalidFormat: msg.SlugInvalidFormat()
            );
    }
}
