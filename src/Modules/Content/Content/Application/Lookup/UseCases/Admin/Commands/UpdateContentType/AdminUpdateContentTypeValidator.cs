using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateContentType;

/// <summary>
/// Validator for the <see cref="AdminUpdateContentTypeCommand" /> ensuring proper content type data format.
/// </summary>
public class AdminUpdateContentTypeValidator : AbstractValidator<AdminUpdateContentTypeCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdateContentTypeValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content type validation error messages.</param>
    public AdminUpdateContentTypeValidator(ContentTypeErrorMessage i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Localizer);
        RuleFor(x => x.Name).ValidContentTypeName(i18n);
    }
}
