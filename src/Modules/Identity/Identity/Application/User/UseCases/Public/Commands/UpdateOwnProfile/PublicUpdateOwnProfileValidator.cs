using _116.Identity.Application.Auth.Validators;

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
    /// Configure validation rules for profile update.
    /// </summary>
    public PublicUpdateOwnProfileValidator()
    {
        RuleFor(x => x.Email).ValidEmail(false);
        RuleFor(x => x.UserName).ValidUsername(false);
        RuleFor(x => x.CountryName).ValidCountryName();
        RuleFor(x => x.CountryIsoCode).ValidCountryIsoCode();
        RuleFor(x => x.CountryDialCode).ValidCountryDialCode();
        RuleFor(x => x.PartialPhoneNumber).ValidPartialPhoneNumber();
    }
}
