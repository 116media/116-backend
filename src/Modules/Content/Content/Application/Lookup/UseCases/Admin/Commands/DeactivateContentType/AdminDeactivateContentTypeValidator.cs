using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivateContentType;

/// <summary>
/// Validator for the <see cref="AdminDeactivateContentTypeCommand" /> ensuring a valid content type ID is provided.
/// </summary>
public class AdminDeactivateContentTypeValidator : AbstractValidator<AdminDeactivateContentTypeCommand>
{
    /// <summary>
    /// Configures validation rules for content type deactivation.
    /// </summary>
    public AdminDeactivateContentTypeValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Content type ID");
    }
}
