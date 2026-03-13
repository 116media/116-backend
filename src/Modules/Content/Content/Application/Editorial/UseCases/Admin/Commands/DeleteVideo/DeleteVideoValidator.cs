using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteVideo;

/// <summary>
/// Validator for the <see cref="DeleteVideoCommand" /> ensuring a valid video ID is provided.
/// </summary>
public class DeleteVideoValidator : AbstractValidator<DeleteVideoCommand>
{
    /// <summary>
    /// Configures validation rules for video deletion.
    /// </summary>
    public DeleteVideoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Video ID");
    }
}
