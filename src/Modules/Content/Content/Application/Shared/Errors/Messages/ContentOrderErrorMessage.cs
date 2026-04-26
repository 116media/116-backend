namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>ContentOrder</c> and <c>ContentPayment</c> domains.
/// </summary>
public static class ContentOrderErrorMessage
{
    /// <summary>
    /// Gets an error message for when the order has already been submitted.
    /// </summary>
    /// <returns>An error message indicating that the order has already been submitted.</returns>
    public static string AlreadySubmitted()
    {
        return "Order has already been submitted.";
    }

    /// <summary>
    /// Gets an error message for when the order has already been paid.
    /// </summary>
    /// <returns>An error message indicating that the order is already paid.</returns>
    public static string AlreadyPaid()
    {
        return "Order is already paid.";
    }

    /// <summary>
    /// Gets an error message for when the order has already been cancelled.
    /// </summary>
    /// <returns>An error message indicating that the order has already been cancelled.</returns>
    public static string AlreadyCancelled()
    {
        return "Order has already been cancelled.";
    }

    /// <summary>
    /// Gets an error message for when an attempt is made to cancel a paid order.
    /// </summary>
    /// <returns>An error message indicating that a paid order cannot be cancelled.</returns>
    public static string CannotCancelPaidOrder()
    {
        return "A paid order cannot be cancelled.";
    }

    /// <summary>
    /// Gets an error message for when an attempt is made to modify a non-draft order.
    /// </summary>
    /// <returns>An error message indicating that only draft orders can be modified.</returns>
    public static string CannotAddItemToNonDraftOrder()
    {
        return "This order can only be modified while in Draft status.";
    }

    /// <summary>
    /// Gets an error message for when a submit is attempted on an order with no priced items.
    /// </summary>
    /// <returns>An error message indicating that at least one item with one pricing tier is required.</returns>
    public static string MustHaveAtLeastOneItemWithTier()
    {
        return "The order must have at least one item with at least one pricing tier before it can be submitted.";
    }

    /// <summary>
    /// Gets an error message for when the payment has already been verified.
    /// </summary>
    /// <returns>An error message indicating that the payment has already been verified.</returns>
    public static string PaymentAlreadyVerified()
    {
        return "Payment has already been verified.";
    }

    /// <summary>
    /// Gets an error message for when the payment has already been rejected.
    /// </summary>
    /// <returns>An error message indicating that the payment has already been rejected.</returns>
    public static string PaymentAlreadyRejected()
    {
        return "Payment has already been rejected.";
    }

    /// <summary>
    /// Gets an error message for when a pricing tier is already attached to an order item.
    /// </summary>
    /// <returns>
    /// An error message indicating that the tier is already attached to the item.
    /// </returns>
    public static string TierAlreadyAttached()
    {
        return "This pricing tier is already attached to the item.";
    }
}
