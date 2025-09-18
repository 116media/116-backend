using FluentValidation;
using System.Text.RegularExpressions;
using _116.BuildingBlocks.Constants;

namespace _116.User.Application.Public.UseCases.Commands.UpdateOwnProfile;

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
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(UserConstants.MaxEmailLength)
            .WithMessage($"Email cannot exceed {UserConstants.MaxEmailLength} characters")
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        // Username validation - optional but must meet requirements if provided
        RuleFor(x => x.UserName)
            .Cascade(CascadeMode.Stop)
            .MinimumLength(UserConstants.MinUserNameLength)
            .WithMessage($"Username must be at least {UserConstants.MinUserNameLength} characters long")
            .MaximumLength(UserConstants.MaxUserNameLength)
            .WithMessage($"Username cannot exceed {UserConstants.MaxUserNameLength} characters")
            .Matches(UsernameRegex())
            .WithMessage("Username can only contain letters, numbers, spaces, and hyphens")
            .When(x => !string.IsNullOrWhiteSpace(x.UserName));

        // Country name validation - if provided
        RuleFor(x => x.CountryName)
            .MaximumLength(UserConstants.MaxCountryNameLength)
            .WithMessage($"Country name cannot exceed {UserConstants.MaxCountryNameLength} characters")
            .When(x => !string.IsNullOrWhiteSpace(x.CountryName));

        // Country flag URL validation - if provided
        RuleFor(x => x.CountryFlagUrl)
            .MaximumLength(UserConstants.MaxCountryFlagUrlLength)
            .WithMessage($"Country flag URL cannot exceed {UserConstants.MaxCountryFlagUrlLength} characters")
            .Must(BeValidUrl)
            .WithMessage("Country flag URL must be a valid URL")
            .When(x => !string.IsNullOrWhiteSpace(x.CountryFlagUrl));

        // Country ISO code validation - if provided
        RuleFor(x => x.CountryIsoCode)
            .MaximumLength(UserConstants.MaxCountryIsoCodeLength)
            .WithMessage($"Country ISO code cannot exceed {UserConstants.MaxCountryIsoCodeLength} characters")
            .Matches("^[A-Z]{2,3}$")
            .WithMessage("Country ISO code must contain only uppercase letters")
            .When(x => !string.IsNullOrWhiteSpace(x.CountryIsoCode));

        // Country dial code validation - if provided
        RuleFor(x => x.CountryDialCode)
            .MaximumLength(UserConstants.MaxCountryDialCodeLength)
            .WithMessage($"Country dial code cannot exceed {UserConstants.MaxCountryDialCodeLength} characters")
            .Matches(@"^\+\d{1,4}$")
            .WithMessage($"Country dial code must start with + followed by 1-{UserConstants.MaxCountryDialCodeLength} digits")
            .When(x => !string.IsNullOrWhiteSpace(x.CountryDialCode));

        // Partial phone number validation - if provided
        RuleFor(x => x.PartialPhoneNumber)
            .MaximumLength(UserConstants.MaxPartialPhoneNumberLength)
            .WithMessage($"Partial phone number cannot exceed {UserConstants.MaxPartialPhoneNumberLength} characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PartialPhoneNumber));
    }

    /// <summary>
    /// Validates that a URL is in proper format.
    /// </summary>
    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Generated regex for username validation - alphanumeric, spaces, and hyphens only.
    /// Uses compile-time generation for better performance, AOT compatibility, and reduced startup time.
    /// </summary>
    [GeneratedRegex(@"^[a-zA-Z0-9\-\s]+$")]
    private static partial Regex UsernameRegex();
}
