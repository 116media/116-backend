using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteShortVideo;

/// <summary>
/// Validator for the <see cref="DeleteShortVideoCommand" /> ensuring a valid short video ID is provided.
/// </summary>
public class DeleteShortVideoValidator : AbstractValidator<DeleteShortVideoCommand>
{
    /// <summary>
    /// Configures validation rules for short video deletion.
    /// </summary>
    public DeleteShortVideoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Short Video ID");
    }
}
