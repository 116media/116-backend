using _116.Content.Application.Shared.Errors.Messages;
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
    /// <param name="i18n">Lyrics validation error messages.</param>
    public AdminUpdateLyricsSeoValidator(LyricsErrorMessage i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Localizer);
    }
}
