using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision;

/// <summary>
/// Validator for the <see cref="PublicVoteOnTranslationRevisionCommand" />.
/// </summary>
public class PublicVoteOnTranslationRevisionValidator : AbstractValidator<PublicVoteOnTranslationRevisionCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicVoteOnTranslationRevisionValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public PublicVoteOnTranslationRevisionValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Comment).ValidTranslationVoteComment(i18n.Translation.Msg);
    }
}
