using _116.Content.Application.Shared.Errors.Facade;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.ProposeLyricsRevision;

/// <summary>
/// Validator for the <see cref="PublicProposeLyricsRevisionCommand" /> ensuring the proposed
/// replacement text is present.
/// </summary>
public class PublicProposeLyricsRevisionValidator : AbstractValidator<PublicProposeLyricsRevisionCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicProposeLyricsRevisionValidator" /> with
    /// the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public PublicProposeLyricsRevisionValidator(ContentI18n i18n)
    {
        RuleFor(x => x.ProposedText).NotEmpty().WithMessage(i18n.LyricsRevision.Msg.ProposedTextRequired());
    }
}
