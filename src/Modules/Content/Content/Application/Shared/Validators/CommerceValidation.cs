using _116.Content.Application.Shared.Errors.Messages;
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
    /// <param name="msg">The error message provider for content order validation messages.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidReceiptUrl<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ContentOrderErrorMessage msg
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(msg.ReceiptUrlRequired())
            .MaximumLength(500)
            .WithMessage(msg.ReceiptUrlTooLong(500));
    }

    /// <summary>
    /// Validates a payment proof file — must not be null.
    /// Size and MIME type constraints are enforced by the upload service.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the payment proof file property.</param>
    /// <param name="msg">The error message provider for content order validation messages.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, IFormFile?> ValidPaymentProofFile<T>(
        this IRuleBuilder<T, IFormFile?> ruleBuilder,
        ContentOrderErrorMessage msg
    )
    {
        return ruleBuilder.NotNull().WithMessage(msg.PaymentProofRequired());
    }

    /// <summary>
    /// Validates that a content kind is restricted to Article or Video only.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the content kind property.</param>
    /// <param name="msg">The error message provider for content order validation messages.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, EnumCoreContentType> ValidOrderItemContentKind<T>(
        this IRuleBuilder<T, EnumCoreContentType> ruleBuilder,
        ContentOrderErrorMessage msg
    )
    {
        return ruleBuilder
            .Must(kind => kind is EnumCoreContentType.Article or EnumCoreContentType.Video)
            .WithMessage(msg.InvalidOrderItemContentKind());
    }

    /// <summary>
    /// Validates that the admin user ID is not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the admin user ID property.</param>
    /// <param name="msg">The error message provider for content order validation messages.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, Guid> ValidAdminUserId<T>(
        this IRuleBuilder<T, Guid> ruleBuilder,
        ContentOrderErrorMessage msg
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(msg.AdminUserIdRequired());
    }

    /// <summary>
    /// Validates that the payment method is a defined <see cref="EnumPaymentMethod"/> value.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the payment method property.</param>
    /// <param name="msg">The error message provider for content order validation messages.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, EnumPaymentMethod> ValidPaymentMethod<T>(
        this IRuleBuilder<T, EnumPaymentMethod> ruleBuilder,
        ContentOrderErrorMessage msg
    )
    {
        return ruleBuilder.IsInEnum().WithMessage(msg.InvalidPaymentMethod());
    }
}
