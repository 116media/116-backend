using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RequestLyricsSubmissionRevision;

/// <summary>
/// Validator for the <see cref="AdminRequestLyricsSubmissionRevisionCommand" /> ensuring a
/// revision-request note is provided.
/// </summary>
public class AdminRequestLyricsSubmissionRevisionValidator
    : AbstractValidator<AdminRequestLyricsSubmissionRevisionCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminRequestLyricsSubmissionRevisionValidator" />
    /// with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminRequestLyricsSubmissionRevisionValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Note).ValidRejectionReason(i18n.Lyrics.Msg);
    }
}
