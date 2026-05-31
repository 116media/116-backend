using _116.Content.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivateContentType;

/// <summary>
/// Validator for the <see cref="AdminDeactivateContentTypeCommand" /> ensuring a valid content type ID is provided.
/// </summary>
public class AdminDeactivateContentTypeValidator : AbstractValidator<AdminDeactivateContentTypeCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminDeactivateContentTypeValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminDeactivateContentTypeValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.ContentType.Msg.Localizer);
    }
}
