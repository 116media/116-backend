using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteLyrics;

/// <summary>
/// Validator for the <see cref="AdminDeleteLyricsCommand" /> ensuring a valid lyrics ID is provided.
/// </summary>
public class AdminDeleteLyricsValidator : AbstractValidator<AdminDeleteLyricsCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminDeleteLyricsValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Lyrics validation error messages.</param>
    public AdminDeleteLyricsValidator(LyricsErrorMessage i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Localizer);
    }
}
