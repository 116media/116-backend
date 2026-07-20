using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission;

/// <summary>
/// Validator for the <see cref="AdminApproveLyricsSubmissionCommand" /> ensuring the assigned
/// slug is present and well-formed.
/// </summary>
public class AdminApproveLyricsSubmissionValidator : AbstractValidator<AdminApproveLyricsSubmissionCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminApproveLyricsSubmissionValidator" /> with
    /// the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminApproveLyricsSubmissionValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Slug).ValidLyricsSlug(i18n.Lyrics.Msg);
    }
}
