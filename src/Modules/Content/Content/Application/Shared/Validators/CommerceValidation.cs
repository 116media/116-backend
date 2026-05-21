using _116.Content.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Shared.Validators;

/// <summary>
/// Shared validation extension methods for Commerce use cases.
/// </summary>
public static class CommerceValidation
{
    /// <summary>
    /// Validates a payment receipt URL — required, max 500 characters.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the receipt URL property.</param>
    /// <param name="receiptUrlRequired">Error message used when the receipt URL is empty.</param>
    /// <param name="receiptUrlTooLong">Error message used when the receipt URL exceeds the maximum length.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidReceiptUrl<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string receiptUrlRequired,
        string receiptUrlTooLong
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(receiptUrlRequired)
            .MaximumLength(500)
            .WithMessage(receiptUrlTooLong);
    }

    /// <summary>
    /// Validates a payment proof file — must not be null.
    /// Size and MIME type constraints are enforced by the upload service.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the payment proof file property.</param>
    /// <param name="paymentProofRequired">Error message used when the payment proof file is null.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, IFormFile?> ValidPaymentProofFile<T>(
        this IRuleBuilder<T, IFormFile?> ruleBuilder,
        string paymentProofRequired
    )
    {
        return ruleBuilder.NotNull().WithMessage(paymentProofRequired);
    }

    /// <summary>
    /// Validates that a content kind is restricted to Article or Video only.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the content kind property.</param>
    /// <param name="invalidOrderItemContentKind">Error message used when the content kind is not Article or Video.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, EnumCoreContentType> ValidOrderItemContentKind<T>(
        this IRuleBuilder<T, EnumCoreContentType> ruleBuilder,
        string invalidOrderItemContentKind
    )
    {
        return ruleBuilder
            .Must(kind => kind is EnumCoreContentType.Article or EnumCoreContentType.Video)
            .WithMessage(invalidOrderItemContentKind);
    }

    /// <summary>
    /// Validates that the admin user ID is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the admin user ID property.</param>
    /// <param name="adminUserIdRequired">Error message used when the admin user ID is empty.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, Guid> ValidAdminUserId<T>(
        this IRuleBuilder<T, Guid> ruleBuilder,
        string adminUserIdRequired
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(adminUserIdRequired);
    }

    /// <summary>
    /// Validates that the payment method is a defined <see cref="EnumPaymentMethod"/> value.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the payment method property.</param>
    /// <param name="invalidPaymentMethod">Error message used when the payment method is not a valid enum value.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, EnumPaymentMethod> ValidPaymentMethod<T>(
        this IRuleBuilder<T, EnumPaymentMethod> ruleBuilder,
        string invalidPaymentMethod
    )
    {
        return ruleBuilder.IsInEnum().WithMessage(invalidPaymentMethod);
    }
}
