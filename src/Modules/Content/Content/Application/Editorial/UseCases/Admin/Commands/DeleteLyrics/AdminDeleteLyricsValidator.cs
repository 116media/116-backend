using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteLyrics;

/// <summary>
/// Validator for the <see cref="AdminDeleteLyricsCommand" /> ensuring a valid lyrics ID is provided.
/// </summary>
public class AdminDeleteLyricsValidator : AbstractValidator<AdminDeleteLyricsCommand>
{
    /// <summary>
    /// Configures validation rules for lyrics deletion.
    /// </summary>
    public AdminDeleteLyricsValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Lyrics ID");
    }
}
