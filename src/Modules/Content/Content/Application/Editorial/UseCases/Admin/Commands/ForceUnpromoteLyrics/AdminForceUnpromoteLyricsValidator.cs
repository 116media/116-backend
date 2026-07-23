using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteLyrics;

/// <summary>
/// Validator for the <see cref="AdminForceUnpromoteLyricsCommand" />.
/// </summary>
public class AdminForceUnpromoteLyricsValidator : AbstractValidator<AdminForceUnpromoteLyricsCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminForceUnpromoteLyricsValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminForceUnpromoteLyricsValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason)
            .ValidUnpromoteReason(
                reasonRequired: i18n.Lyrics.Msg.RejectionReasonRequired(),
                reasonTooLong: i18n.Lyrics.Msg.RejectionReasonTooLong(ContentConstants.MaxRejectionReasonLength)
            );
    }
}
