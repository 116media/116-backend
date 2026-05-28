using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
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
    /// Initializes a new instance of <see cref="AdminUpdateOwnProfileValidator" /> with validation rules.
    /// </summary>
    /// <param name="i18n">
    /// Validation error messages for rule configuration.
    /// </param>
    public AdminUpdateOwnProfileValidator(ValidationErrorMessage i18n)
    {
        RuleFor(x => x.UserName).ValidUsername(i18n, isRequired: false);
        RuleFor(x => x.CountryName).ValidCountryName(i18n);
        RuleFor(x => x.CountryIsoCode).ValidCountryIsoCode(i18n);
        RuleFor(x => x.CountryDialCode).ValidCountryDialCode(i18n);
        RuleFor(x => x.PartialPhoneNumber).ValidPartialPhoneNumber(i18n);
    }
}
