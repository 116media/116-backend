using FluentValidation;
using _116.Identity.Application.Shared.Validators;

namespace _116.Identity.Application.Public.UseCases.Commands.UpdateOwnProfile;

/// <summary>
/// Validator for the <see cref="PublicUpdateOwnProfileCommand"/> ensuring proper profile update data format.
/// This endpoint requires user authentication - only logged-in users can update their own profile.
/// </summary>
/// <remarks>
/// Validates profile update data according to business requirements:
/// - Email: Valid email format if provided
/// - UserName: Proper length and format if provided
/// - Phone: Valid format with country information if provided
/// - Country: Complete country information if phone is provided
/// </remarks>
public partial class PublicUpdateOwnProfileValidator : AbstractValidator<PublicUpdateOwnProfileCommand>
{
    /// <summary>
    /// Configure validation rules for profile update.
    /// </summary>
    public PublicUpdateOwnProfileValidator()
    {
        // Email validation - optional but must be valid format if provided
        RuleFor(x => x.Email).EmailValidation(isRequired: false);

        // Username validation - optional but must meet requirements if provided
        RuleFor(x => x.UserName).UsernameValidation(isRequired: false);

        // Country name validation - if provided
        RuleFor(x => x.CountryName).CountryNameValidation();

        // Country flag URL validation - if provided
        RuleFor(x => x.CountryFlagUrl).CountryFlagUrlValidation();

        // Country ISO code validation - if provided
        RuleFor(x => x.CountryIsoCode).CountryIsoCodeValidation();

        // Country dial code validation - if provided
        RuleFor(x => x.CountryDialCode).CountryDialCodeValidation();

        // Partial phone number validation - if provided
        RuleFor(x => x.PartialPhoneNumber).PartialPhoneNumberValidation();
    }

}
