using _116.Mailer.Application.Shared.Errors;
using FluentValidation;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Commands.ConfirmNewsletter;

/// <summary>
/// Validator for the <see cref="PublicConfirmNewsletterCommand" /> ensuring a
/// token is present.
/// </summary>
public class PublicConfirmNewsletterValidator : AbstractValidator<PublicConfirmNewsletterCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicConfirmNewsletterValidator" />
    /// with validation rules.
    /// </summary>
    /// <param name="errors">Newsletter error factory providing localized messages.</param>
    public PublicConfirmNewsletterValidator(NewsletterErrors errors)
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage(errors.Msg.TokenInvalid());
    }
}
