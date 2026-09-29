using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyricsSubmission;

/// <summary>
/// Validator for the <see cref="AdminRejectLyricsSubmissionCommand" /> ensuring a rejection
/// note is provided.
/// </summary>
public class AdminRejectLyricsSubmissionValidator : AbstractValidator<AdminRejectLyricsSubmissionCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminRejectLyricsSubmissionValidator" /> with
    /// the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminRejectLyricsSubmissionValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Note).ValidRejectionReason(i18n.Lyrics.Msg);
    }
}
