using _116.Mailer.Application.Shared.Errors;
using FluentValidation;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Commands.UnsubscribeNewsletter;

/// <summary>
/// Validator for the <see cref="PublicUnsubscribeNewsletterCommand" /> ensuring
/// a token is present.
/// </summary>
public class PublicUnsubscribeNewsletterValidator : AbstractValidator<PublicUnsubscribeNewsletterCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicUnsubscribeNewsletterValidator" />
    /// with validation rules.
    /// </summary>
    /// <param name="errors">Newsletter error factory providing localized messages.</param>
    public PublicUnsubscribeNewsletterValidator(NewsletterErrors errors)
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage(errors.Msg.TokenInvalid());
    }
}
