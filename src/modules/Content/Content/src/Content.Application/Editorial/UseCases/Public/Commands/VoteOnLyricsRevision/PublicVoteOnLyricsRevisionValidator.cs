using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnLyricsRevision;

/// <summary>
/// Validator for the <see cref="PublicVoteOnLyricsRevisionCommand" />.
/// </summary>
public class PublicVoteOnLyricsRevisionValidator : AbstractValidator<PublicVoteOnLyricsRevisionCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicVoteOnLyricsRevisionValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public PublicVoteOnLyricsRevisionValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Comment).ValidLyricsRevisionVoteComment(i18n.LyricsRevision.Msg);
    }
}
