using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;

/// <summary>
/// Validator for the <see cref="UpdateLyricsCommand" /> ensuring proper lyrics update data.
/// </summary>
public class UpdateLyricsValidator : AbstractValidator<UpdateLyricsCommand>
{
    /// <summary>
    /// Configures validation rules for lyrics text update.
    /// </summary>
    public UpdateLyricsValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Lyrics ID");

        RuleFor(x => x.LyricsText).ValidLyricsText();
    }
}
