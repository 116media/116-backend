using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveVideo;

/// <summary>
/// Validator for the <see cref="AdminApproveVideoCommand" /> ensuring a valid video ID is provided.
/// </summary>
public class AdminApproveVideoValidator : AbstractValidator<AdminApproveVideoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminApproveVideoValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Video validation error messages.</param>
    public AdminApproveVideoValidator(VideoErrorMessage i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Localizer);
    }
}
