using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag;

/// <summary>
/// Validator for the <see cref="AdminUpdateTagCommand" /> ensuring proper tag data format.
/// </summary>
public class AdminUpdateTagValidator : AbstractValidator<AdminUpdateTagCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdateTagValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUpdateTagValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Tag.Msg.Localizer);
        RuleFor(x => x.Name).ValidTagName(i18n.Tag.Msg);
        RuleFor(x => x.Slug).ValidTagSlug(i18n.Tag.Msg);
    }
}
