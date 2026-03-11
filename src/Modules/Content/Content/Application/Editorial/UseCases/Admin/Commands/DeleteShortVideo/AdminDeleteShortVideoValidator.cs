using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteShortVideo;

/// <summary>
/// Validator for the <see cref="AdminDeleteShortVideoCommand" /> ensuring a valid short video ID is provided.
/// </summary>
public class AdminDeleteShortVideoValidator : AbstractValidator<AdminDeleteShortVideoCommand>
{
    /// <summary>
    /// Configures validation rules for short video deletion.
    /// </summary>
    public AdminDeleteShortVideoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Short Video ID");
    }
}
