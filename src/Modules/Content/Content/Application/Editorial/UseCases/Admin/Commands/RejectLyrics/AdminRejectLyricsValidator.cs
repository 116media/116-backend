using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyrics;

/// <summary>
/// Validator for the <see cref="AdminRejectLyricsCommand" /> ensuring a rejection reason is provided.
/// </summary>
public class AdminRejectLyricsValidator : AbstractValidator<AdminRejectLyricsCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminRejectLyricsValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminRejectLyricsValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Lyrics.Msg.Localizer);

        RuleFor(x => x.Reason).ValidRejectionReason(i18n.Lyrics.Msg);
    }
}
