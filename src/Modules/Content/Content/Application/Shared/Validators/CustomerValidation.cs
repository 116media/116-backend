using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Shared.Validators;

/// <summary>
/// FluentValidation extensions for customer field validation (id, fullName, email, phone, company, notes).
/// </summary>
public static class CustomerValidation
{
    /// <summary>
    /// Validates customer full name with length constraints.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidCustomerFullName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Customer full name is required.")
            .MaximumLength(maximumLength: ContentConstants.MaxCustomerFullNameLength)
            .WithMessage(
                $"Customer full name must not exceed {ContentConstants.MaxCustomerFullNameLength} characters."
            );
    }

    /// <summary>
    /// Validates customer email with format and length constraints.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidCustomerEmail<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Customer email is required.")
            .EmailAddress()
            .WithMessage("Customer email must be a valid email address.")
            .MaximumLength(maximumLength: ContentConstants.MaxCustomerEmailLength)
            .WithMessage($"Customer email must not exceed {ContentConstants.MaxCustomerEmailLength} characters.");
    }

    /// <summary>
    /// Validates optional customer phone with length constraints.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidCustomerPhone<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(maximumLength: ContentConstants.MaxCustomerPhoneLength)
            .WithMessage($"Customer phone must not exceed {ContentConstants.MaxCustomerPhoneLength} characters.")
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Phone")));
    }

    /// <summary>
    /// Validates optional customer company with length constraints.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidCustomerCompany<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(maximumLength: ContentConstants.MaxCustomerCompanyLength)
            .WithMessage($"Customer company must not exceed {ContentConstants.MaxCustomerCompanyLength} characters.")
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Company")));
    }

    /// <summary>
    /// Validates optional customer notes with length constraints.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidCustomerNotes<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(maximumLength: ContentConstants.MaxCustomerNotesLength)
            .WithMessage($"Customer notes must not exceed {ContentConstants.MaxCustomerNotesLength} characters.")
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Notes")));
    }
}
