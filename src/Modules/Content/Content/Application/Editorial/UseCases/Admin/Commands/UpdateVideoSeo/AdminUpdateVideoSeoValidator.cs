using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo;

/// <summary>
/// Validator for the <see cref="AdminUpdateVideoSeoCommand" /> ensuring a valid video ID is provided.
/// </summary>
public class AdminUpdateVideoSeoValidator : AbstractValidator<AdminUpdateVideoSeoCommand>
{
    /// <summary>
    /// Configures validation rules for video SEO update.
    /// </summary>
    public AdminUpdateVideoSeoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Video ID");
    }
}
