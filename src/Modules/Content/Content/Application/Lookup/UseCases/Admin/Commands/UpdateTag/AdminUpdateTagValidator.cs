using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag;

/// <summary>
/// Validator for the <see cref="AdminUpdateTagCommand" /> ensuring proper tag data format.
/// </summary>
public class AdminUpdateTagValidator : AbstractValidator<AdminUpdateTagCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdateTagValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="msg">Tag validation error messages.</param>
    public AdminUpdateTagValidator(TagErrorMessage msg)
    {
        RuleFor(x => x.Id).IsValidGuid("Tag ID");
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
