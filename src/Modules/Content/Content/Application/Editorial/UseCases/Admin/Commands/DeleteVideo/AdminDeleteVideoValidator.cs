using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteVideo;

/// <summary>
/// Validator for the <see cref="AdminDeleteVideoCommand" /> ensuring a valid video ID is provided.
/// </summary>
public class AdminDeleteVideoValidator : AbstractValidator<AdminDeleteVideoCommand>
{
    /// <summary>
    /// Configures validation rules for video deletion.
    /// </summary>
    public AdminDeleteVideoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Video ID");
    }
}
