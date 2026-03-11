using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;

/// <summary>
/// Validator for the <see cref="AdminUpdateLyricsCommand" /> ensuring proper lyrics update data.
/// </summary>
public class AdminUpdateLyricsValidator : AbstractValidator<AdminUpdateLyricsCommand>
{
    /// <summary>
    /// Configures validation rules for lyrics text update.
    /// </summary>
    public AdminUpdateLyricsValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Lyrics ID");

        RuleFor(x => x.LyricsText).ValidLyricsText();
    }
}
