using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateContentType;

/// <summary>
/// Validator for the <see cref="UpdateContentTypeCommand" /> ensuring proper content type data format.
/// </summary>
public class UpdateContentTypeValidator : AbstractValidator<UpdateContentTypeCommand>
{
    /// <summary>
    /// Configures validation rules for content type update.
    /// </summary>
    public UpdateContentTypeValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Content type ID");
        RuleFor(x => x.Name).ValidContentTypeName();
    }
}
