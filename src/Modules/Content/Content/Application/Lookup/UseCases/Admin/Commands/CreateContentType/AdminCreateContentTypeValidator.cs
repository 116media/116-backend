using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType;

/// <summary>
/// Validator for the <see cref="AdminCreateContentTypeCommand" /> ensuring proper content type data format.
/// </summary>
public class AdminCreateContentTypeValidator : AbstractValidator<AdminCreateContentTypeCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateContentTypeValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="msg">Content type validation error messages.</param>
    public AdminCreateContentTypeValidator(ContentTypeErrorMessage msg)
    {
        RuleFor(x => x.Name)
            .ValidContentTypeName(
                nameRequired: msg.NameRequired(),
                nameTooLong: msg.NameTooLong(ContentConstants.MaxContentTypeNameLength)
            );
    }
}
