using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Facade;
using FluentValidation;

namespace _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile;

/// <summary>
/// Validator for the <see cref="PublicUpdateOwnProfileCommand" /> ensuring proper profile update data format.
/// This endpoint requires user authentication - only logged-in users can update their own profile.
/// </summary>
/// <remarks>
/// Validates profile update data according to business requirements:
/// - Email: Valid email format if provided
/// - UserName: Proper length and format if provided
/// - Phone: Valid format with country information if provided
/// - Country: Complete country information if phone is provided
/// </remarks>
public class PublicUpdateOwnProfileValidator : AbstractValidator<PublicUpdateOwnProfileCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicUpdateOwnProfileValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public PublicUpdateOwnProfileValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.Email).ValidEmail(i18n.User.Validation, isRequired: false);
        RuleFor(x => x.UserName).ValidUsername(i18n.User.Validation, isRequired: false);
        RuleFor(x => x.CountryName).ValidCountryName(i18n.User.Validation);
        RuleFor(x => x.CountryIsoCode).ValidCountryIsoCode(i18n.User.Validation);
        RuleFor(x => x.CountryDialCode).ValidCountryDialCode(i18n.User.Validation);
        RuleFor(x => x.PartialPhoneNumber).ValidPartialPhoneNumber(i18n.User.Validation);
    }
}
