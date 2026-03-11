using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo;

/// <summary>
/// Validator for the <see cref="AdminUpdateLyricsSeoCommand" /> ensuring a valid lyrics ID is provided.
/// </summary>
public class AdminUpdateLyricsSeoValidator : AbstractValidator<AdminUpdateLyricsSeoCommand>
{
    /// <summary>
    /// Configures validation rules for lyrics SEO update.
    /// </summary>
    public AdminUpdateLyricsSeoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Lyrics ID");
    }
}
