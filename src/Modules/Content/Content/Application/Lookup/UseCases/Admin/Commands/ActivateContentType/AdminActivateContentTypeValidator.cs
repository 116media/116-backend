using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivateContentType;

/// <summary>
/// Validator for the <see cref="AdminActivateContentTypeCommand" /> ensuring a valid content type ID is provided.
/// </summary>
public class AdminActivateContentTypeValidator : AbstractValidator<AdminActivateContentTypeCommand>
{
    /// <summary>
    /// Configures validation rules for content type activation.
    /// </summary>
    public AdminActivateContentTypeValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Content type ID");
    }
}
