using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivateContentType;

/// <summary>
/// Validator for the <see cref="ActivateContentTypeCommand" /> ensuring a valid content type ID is provided.
/// </summary>
public class ActivateContentTypeValidator : AbstractValidator<ActivateContentTypeCommand>
{
    /// <summary>
    /// Configures validation rules for content type activation.
    /// </summary>
    public ActivateContentTypeValidator()
    {
        RuleFor(x => x.Id).ValidContentTypeId();
    }
}
