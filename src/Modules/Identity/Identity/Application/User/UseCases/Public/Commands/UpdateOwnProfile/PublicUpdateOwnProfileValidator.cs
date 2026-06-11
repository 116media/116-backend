using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
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
    /// <param name="msg">
    /// Validation error messages for rule configuration.
    /// </param>
    public PublicUpdateOwnProfileValidator(ValidationErrorMessage msg)
    {
        RuleFor(x => x.Email)
            .ValidEmail(
                emailRequired: msg.EmailRequired(),
                emailTooLong: msg.EmailTooLong(UserConstants.MaxEmailLength),
                invalidEmailFormat: msg.InvalidEmailFormatMsg(),
                isRequired: false
            );
        RuleFor(x => x.UserName)
            .ValidUsername(
                usernameRequired: msg.UsernameRequired(),
                usernameTooShort: msg.UsernameTooShort(UserConstants.MinUserNameLength),
                usernameTooLong: msg.UsernameTooLong(UserConstants.MaxUserNameLength),
                usernameInvalidChars: msg.UsernameInvalidChars(),
                isRequired: false
            );
        RuleFor(x => x.CountryName)
            .ValidCountryName(countryNameTooLong: msg.CountryNameTooLong(UserConstants.MaxCountryNameLength));
        RuleFor(x => x.CountryIsoCode)
            .ValidCountryIsoCode(
                countryIsoCodeTooLong: msg.CountryIsoCodeTooLong(UserConstants.MaxCountryIsoCodeLength),
                countryIsoCodeInvalid: msg.CountryIsoCodeInvalid()
            );
        RuleFor(x => x.CountryDialCode)
            .ValidCountryDialCode(
                countryDialCodeTooLong: msg.CountryDialCodeTooLong(UserConstants.MaxCountryDialCodeLength),
                countryDialCodeInvalid: msg.CountryDialCodeInvalid(UserConstants.MaxCountryDialCodeLength)
            );
        RuleFor(x => x.PartialPhoneNumber)
            .ValidPartialPhoneNumber(
                partialPhoneNumberTooLong: msg.PartialPhoneNumberTooLong(UserConstants.MaxPartialPhoneNumberLength)
            );
    }
}
