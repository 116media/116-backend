using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo;

/// <summary>
/// Validator for the <see cref="UpdateLyricsSeoCommand" /> ensuring a valid lyrics ID is provided.
/// </summary>
public class UpdateLyricsSeoValidator : AbstractValidator<UpdateLyricsSeoCommand>
{
    /// <summary>
    /// Configures validation rules for lyrics SEO update.
    /// </summary>
    public UpdateLyricsSeoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Lyrics ID");
    }
}
