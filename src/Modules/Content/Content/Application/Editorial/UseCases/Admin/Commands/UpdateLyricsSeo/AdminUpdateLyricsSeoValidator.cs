using _116.Content.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo;

/// <summary>
/// Validator for the <see cref="AdminUpdateLyricsSeoCommand" /> ensuring a valid lyrics ID is provided.
/// </summary>
public class AdminUpdateLyricsSeoValidator : AbstractValidator<AdminUpdateLyricsSeoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdateLyricsSeoValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUpdateLyricsSeoValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Lyrics.Msg.Localizer);
    }
}
