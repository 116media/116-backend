using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType;

/// <summary>
/// Validator for the <see cref="AdminCreateContentTypeCommand" /> ensuring proper content type data format.
/// </summary>
public class AdminCreateContentTypeValidator : AbstractValidator<AdminCreateContentTypeCommand>
{
    /// <summary>
    /// Configures validation rules for content type creation.
    /// </summary>
    public AdminCreateContentTypeValidator()
    {
        RuleFor(x => x.Name).ValidContentTypeName();
    }
}
