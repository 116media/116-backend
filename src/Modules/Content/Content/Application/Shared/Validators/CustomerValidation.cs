using _116.Content.Application.Shared.Errors.Messages;
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
    /// <param name="msg">The error message provider for customer validation messages.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCustomerFullName<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        CustomerErrorMessage msg
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(msg.FullNameRequired())
            .MaximumLength(maximumLength: ContentConstants.MaxCustomerFullNameLength)
            .WithMessage(msg.FullNameTooLong(ContentConstants.MaxCustomerFullNameLength));
    }

    /// <summary>
    /// Validates customer email with format and length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the email property.</param>
    /// <param name="msg">The error message provider for customer validation messages.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCustomerEmail<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        CustomerErrorMessage msg
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(msg.EmailRequired())
            .EmailAddress()
            .WithMessage(msg.EmailInvalidFormat())
            .MaximumLength(maximumLength: ContentConstants.MaxCustomerEmailLength)
            .WithMessage(msg.EmailTooLong(ContentConstants.MaxCustomerEmailLength));
    }

    /// <summary>
    /// Validates optional customer phone with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the phone property.</param>
    /// <param name="msg">The error message provider for customer validation messages.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCustomerPhone<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        CustomerErrorMessage msg
    )
    {
        return ruleBuilder
            .MaximumLength(maximumLength: ContentConstants.MaxCustomerPhoneLength)
            .WithMessage(msg.PhoneTooLong(ContentConstants.MaxCustomerPhoneLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Phone")));
    }

    /// <summary>
    /// Validates optional customer company with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the company property.</param>
    /// <param name="msg">The error message provider for customer validation messages.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCustomerCompany<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        CustomerErrorMessage msg
    )
    {
        return ruleBuilder
            .MaximumLength(maximumLength: ContentConstants.MaxCustomerCompanyLength)
            .WithMessage(msg.CompanyTooLong(ContentConstants.MaxCustomerCompanyLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Company")));
    }

    /// <summary>
    /// Validates optional customer notes with length constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the notes property.</param>
    /// <param name="msg">The error message provider for customer validation messages.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidCustomerNotes<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        CustomerErrorMessage msg
    )
    {
        return ruleBuilder
            .MaximumLength(maximumLength: ContentConstants.MaxCustomerNotesLength)
            .WithMessage(msg.NotesTooLong(ContentConstants.MaxCustomerNotesLength))
            .When(x => !string.IsNullOrWhiteSpace(ValidationUtils.GetPropertyValue(instance: x, "Notes")));
    }
}
