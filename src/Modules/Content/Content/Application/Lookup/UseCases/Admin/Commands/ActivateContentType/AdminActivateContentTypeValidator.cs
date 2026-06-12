using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivateContentType;

/// <summary>
/// Validator for the <see cref="AdminActivateContentTypeCommand" /> ensuring a valid content type ID is provided.
/// </summary>
public class AdminActivateContentTypeValidator : AbstractValidator<AdminActivateContentTypeCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminActivateContentTypeValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content type validation error messages.</param>
    public AdminActivateContentTypeValidator(ContentTypeErrorMessage i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Localizer);
    }
}
