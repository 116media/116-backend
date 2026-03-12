using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivateContentType;

/// <summary>
/// Validator for the <see cref="DeactivateContentTypeCommand" /> ensuring a valid content type ID is provided.
/// </summary>
public class DeactivateContentTypeValidator : AbstractValidator<DeactivateContentTypeCommand>
{
    /// <summary>
    /// Configures validation rules for content type deactivation.
    /// </summary>
    public DeactivateContentTypeValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Content type ID");
    }
}
