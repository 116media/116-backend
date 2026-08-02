using _116.Content.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitLyrics;

/// <summary>
/// Validator for the <see cref="AdminSubmitLyricsCommand" /> ensuring a valid lyrics ID is provided.
/// </summary>
public class AdminSubmitLyricsValidator : AbstractValidator<AdminSubmitLyricsCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminSubmitLyricsValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminSubmitLyricsValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Lyrics.Msg.Localizer);
    }
}
