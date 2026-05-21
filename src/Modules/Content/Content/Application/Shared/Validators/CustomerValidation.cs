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
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the full name property.</param>
    /// <param name="fullNameRequired">Error message used when the full name is empty.</param>
    /// <param name="fullNameTooLong">Error message used when the full name exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCustomerFullName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string fullNameRequired,
        string fullNameTooLong
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(fullNameRequired)
            .MaximumLength(maximumLength: ContentConstants.MaxCustomerFullNameLength)
            .WithMessage(fullNameTooLong);
    }

    /// <summary>
    /// Validates customer email with format and length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the email property.</param>
    /// <param name="emailRequired">Error message used when the email is empty.</param>
    /// <param name="emailInvalidFormat">Error message used when the email format is invalid.</param>
    /// <param name="emailTooLong">Error message used when the email exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCustomerEmail<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string emailRequired,
        string emailInvalidFormat,
        string emailTooLong
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(emailRequired)
            .EmailAddress()
            .WithMessage(emailInvalidFormat)
            .MaximumLength(maximumLength: ContentConstants.MaxCustomerEmailLength)
            .WithMessage(emailTooLong);
    }

    /// <summary>
    /// Validates optional customer phone with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the phone property.</param>
    /// <param name="phoneTooLong">Error message used when the phone exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCustomerPhone<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string phoneTooLong
    )
    {
        return ruleBuilder
            .MaximumLength(maximumLength: ContentConstants.MaxCustomerPhoneLength)
            .WithMessage(phoneTooLong)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Phone")));
    }

    /// <summary>
    /// Validates optional customer company with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the company property.</param>
    /// <param name="companyTooLong">Error message used when the company name exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCustomerCompany<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string companyTooLong
    )
    {
        return ruleBuilder
            .MaximumLength(maximumLength: ContentConstants.MaxCustomerCompanyLength)
            .WithMessage(companyTooLong)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Company")));
    }

    /// <summary>
    /// Validates optional customer notes with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the notes property.</param>
    /// <param name="notesTooLong">Error message used when the notes exceed the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCustomerNotes<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string notesTooLong
    )
    {
        return ruleBuilder
            .MaximumLength(maximumLength: ContentConstants.MaxCustomerNotesLength)
            .WithMessage(notesTooLong)
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Notes")));
    }
}
