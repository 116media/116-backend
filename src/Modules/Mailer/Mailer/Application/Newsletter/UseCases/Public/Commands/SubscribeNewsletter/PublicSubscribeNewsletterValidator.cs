using _116.Mailer.Application.Shared.Errors;
using FluentValidation;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Commands.SubscribeNewsletter;

/// <summary>
/// Validator for the <see cref="PublicSubscribeNewsletterCommand" /> ensuring a
/// present, well-formed email address of a storable length.
/// </summary>
public class PublicSubscribeNewsletterValidator : AbstractValidator<PublicSubscribeNewsletterCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicSubscribeNewsletterValidator" />
    /// with validation rules.
    /// </summary>
    /// <param name="errors">Newsletter error factory providing localized messages.</param>
    public PublicSubscribeNewsletterValidator(NewsletterErrors errors)
    {
        RuleFor(x => x.Email)
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(errors.Msg.EmailRequired())
            .EmailAddress()
            .WithMessage(errors.Msg.EmailInvalid())
            .MaximumLength(maximumLength: 320)
            .WithMessage(errors.Msg.EmailInvalid());
    }
}
