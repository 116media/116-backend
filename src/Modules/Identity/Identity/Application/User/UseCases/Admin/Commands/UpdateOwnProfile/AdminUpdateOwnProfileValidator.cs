using _116.Identity.Application.Auth.Validators;

using FluentValidation;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile;

/// <summary>
/// Validator for the <see cref="AdminUpdateOwnProfileCommand" /> ensuring proper profile update data format.
/// This endpoint requires admin user authentication - only logged-in admin users can update their own profile.
/// </summary>
/// <remarks>
/// Validates profile update data according to business requirements:
/// - UserName: Proper length and format if provided
/// - Phone: Valid format with country information if provided
/// - Country: Complete country information if phone is provided
/// Note: Email validation is excluded for admin users.
/// </remarks>
public class AdminUpdateOwnProfileValidator : AbstractValidator<AdminUpdateOwnProfileCommand>
{
    /// <summary>
    /// Configure validation rules for admin profile update.
    /// </summary>
    public AdminUpdateOwnProfileValidator()
    {
        // Username validation - optional but must meet requirements if provided
        RuleFor(x => x.UserName).UsernameValidation(false);
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
