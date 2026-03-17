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
    public static IRuleBuilderOptions<T, string?> ValidReceiptUrl<T>(this IRuleBuilderInitial<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Receipt URL is required.")
            .MaximumLength(500)
            .WithMessage("Receipt URL must not exceed 500 characters.");
    }

    /// <summary>
    /// Validates a payment proof file — must not be null.
    /// Size and MIME type constraints are enforced by the upload service.
    /// </summary>
    public static IRuleBuilderOptions<T, IFormFile?> ValidPaymentProofFile<T>(
        this IRuleBuilder<T, IFormFile?> ruleBuilder
    )
    {
        return ruleBuilder.NotNull().WithMessage("Payment proof file is required.");
    }

    /// <summary>
    /// Validates that a content kind is restricted to Article or Video only.
    /// </summary>
    public static IRuleBuilderOptions<T, EnumCoreContentType> ValidOrderItemContentKind<T>(
        this IRuleBuilder<T, EnumCoreContentType> ruleBuilder
    )
    {
        return ruleBuilder
            .Must(kind => kind is EnumCoreContentType.Article or EnumCoreContentType.Video)
            .WithMessage("Content kind must be Article or Video.");
    }
}
