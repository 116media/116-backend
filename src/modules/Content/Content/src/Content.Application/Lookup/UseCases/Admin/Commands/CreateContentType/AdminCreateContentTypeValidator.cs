using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType;

/// <summary>
/// Validator for the <see cref="AdminCreateContentTypeCommand" /> ensuring proper content type data format.
/// </summary>
public class AdminCreateContentTypeValidator : AbstractValidator<AdminCreateContentTypeCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateContentTypeValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminCreateContentTypeValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Name).ValidContentTypeName(i18n.ContentType.Msg);
    }
}
