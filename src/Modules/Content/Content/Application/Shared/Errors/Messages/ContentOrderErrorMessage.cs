using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>ContentOrder</c> and <c>ContentPayment</c> domains.
/// </summary>
public class ContentOrderErrorMessage(IStringLocalizer<ContentOrderErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Gets an error message for when the order has already been submitted.
    /// </summary>
    /// <returns>An error message indicating that the order has already been submitted.</returns>
    public string AlreadySubmitted()
    {
        return localizer["AlreadySubmitted"];
    }

    /// <summary>
    /// Gets an error message for when the order has already been paid.
    /// </summary>
    /// <returns>An error message indicating that the order is already paid.</returns>
    public string AlreadyPaid()
    {
        return localizer["AlreadyPaid"];
    }

    /// <summary>
    /// Gets an error message for when the order has already been cancelled.
    /// </summary>
    /// <returns>An error message indicating that the order has already been cancelled.</returns>
    public string AlreadyCancelled()
    {
        return localizer["AlreadyCancelled"];
    }

    /// <summary>
    /// Gets an error message for when an attempt is made to cancel a paid order.
    /// </summary>
    /// <returns>An error message indicating that a paid order cannot be cancelled.</returns>
    public string CannotCancelPaidOrder()
    {
        return localizer["CannotCancelPaidOrder"];
    }

    /// <summary>
    /// Gets an error message for when an attempt is made to modify a non-draft order.
    /// </summary>
    /// <returns>An error message indicating that only draft orders can be modified.</returns>
    public string CannotAddItemToNonDraftOrder()
    {
        return localizer["CannotAddItemToNonDraftOrder"];
    }

    /// <summary>
    /// Gets an error message for when a submit is attempted on an order with no priced items.
    /// </summary>
    /// <returns>An error message indicating that at least one item with one pricing tier is required.</returns>
    public string MustHaveAtLeastOneItemWithTier()
    {
        return localizer["MustHaveAtLeastOneItemWithTier"];
    }

    /// <summary>
    /// Gets an error message for when the payment has already been verified.
    /// </summary>
    /// <returns>An error message indicating that the payment has already been verified.</returns>
    public string PaymentAlreadyVerified()
    {
        return localizer["PaymentAlreadyVerified"];
    }

    /// <summary>
    /// Gets an error message for when the payment has already been rejected.
    /// </summary>
    /// <returns>An error message indicating that the payment has already been rejected.</returns>
    public string PaymentAlreadyRejected()
    {
        return localizer["PaymentAlreadyRejected"];
    }

    /// <summary>
    /// Gets an error message for when the promotion duration of a purchased level cannot be resolved.
    /// </summary>
    /// <returns>
    /// An error message indicating that the promotion duration is unavailable.
    /// </returns>
    public string PromotionDurationUnavailable()
    {
        return localizer["PromotionDurationUnavailable"];
    }

    /// <summary>
    /// Gets an error message for when a pricing tier is already attached to an order item.
    /// </summary>
    /// <returns>
    /// An error message indicating that the tier is already attached to the item.
    /// </returns>
    public string TierAlreadyAttached()
    {
        return localizer["TierAlreadyAttached"];
    }

    /// <summary>
    /// Gets an error message for when a receipt URL is required.
    /// </summary>
    public string ReceiptUrlRequired() => localizer["ReceiptUrlRequired"];

    /// <summary>
    /// Gets an error message for when a receipt URL exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string ReceiptUrlTooLong(int max) => string.Format(localizer["ReceiptUrlTooLong"], max);

    /// <summary>
    /// Gets an error message for when a payment proof file is required.
    /// </summary>
    public string PaymentProofRequired() => localizer["PaymentProofRequired"];

    /// <summary>
    /// Gets an error message for when a content kind is invalid for an order item.
    /// </summary>
    public string InvalidOrderItemContentKind() => localizer["InvalidOrderItemContentKind"];

    /// <summary>
    /// Gets an error message for when an admin user ID is required.
    /// </summary>
    public string AdminUserIdRequired() => localizer["AdminUserIdRequired"];

    /// <summary>
    /// Gets an error message for when a payment method is invalid.
    /// </summary>
    public string InvalidPaymentMethod() => localizer["InvalidPaymentMethod"];

    /// <summary>
    /// Gets an error message for when an order item ID is required.
    /// </summary>
    public string OrderItemIdRequired() => localizer["OrderItemIdRequired"];
}
