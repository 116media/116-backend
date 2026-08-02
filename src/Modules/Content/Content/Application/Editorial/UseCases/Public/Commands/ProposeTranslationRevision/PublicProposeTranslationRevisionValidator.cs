using _116.Content.Application.Shared.Errors.Facade;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.ProposeTranslationRevision;

/// <summary>
/// Validator for the <see cref="PublicProposeTranslationRevisionCommand" /> ensuring the
/// proposed replacement text is present.
/// </summary>
public class PublicProposeTranslationRevisionValidator : AbstractValidator<PublicProposeTranslationRevisionCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicProposeTranslationRevisionValidator" />
    /// with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public PublicProposeTranslationRevisionValidator(ContentI18n i18n)
    {
        RuleFor(x => x.ProposedText).NotEmpty().WithMessage(i18n.Translation.Msg.ProposedTextRequired());
    }
}
