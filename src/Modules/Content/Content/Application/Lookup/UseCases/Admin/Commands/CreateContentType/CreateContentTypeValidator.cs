using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType;

/// <summary>
/// Validator for the <see cref="CreateContentTypeCommand" /> ensuring proper content type data format.
/// </summary>
public class CreateContentTypeValidator : AbstractValidator<CreateContentTypeCommand>
{
    /// <summary>
    /// Configures validation rules for content type creation.
    /// </summary>
    public CreateContentTypeValidator()
    {
        RuleFor(x => x.Name).ValidContentTypeName();
    }
}
