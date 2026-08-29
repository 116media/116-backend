using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Facade;
using FluentValidation;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;

/// <summary>
/// Validator for the <see cref="PublicSocialLoginCommand" /> ensuring proper social login data format.
/// </summary>
/// <remarks>
/// Validates only what the client is trusted to send:
/// - Provider: Must be Google or Facebook only
/// - IdToken: Required
/// The verified identity is resolved from the provider token, not validated here.
/// </remarks>
public class PublicSocialLoginValidator : AbstractValidator<PublicSocialLoginCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicSocialLoginValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public PublicSocialLoginValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.Provider).ValidAuthProvider(i18n.User.Validation);
        RuleFor(x => x.IdToken).NotEmpty().WithMessage(i18n.User.Validation.IdTokenRequired());
    }
}
