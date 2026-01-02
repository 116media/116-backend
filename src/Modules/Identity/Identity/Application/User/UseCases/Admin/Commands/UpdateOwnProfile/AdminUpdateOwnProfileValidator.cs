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
        RuleFor(x => x.UserName).ValidUsername(false);
        RuleFor(x => x.CountryName).ValidCountryName();
        RuleFor(x => x.CountryIsoCode).ValidCountryIsoCode();
        RuleFor(x => x.CountryDialCode).ValidCountryDialCode();
        RuleFor(x => x.PartialPhoneNumber).ValidPartialPhoneNumber();
    }
}
