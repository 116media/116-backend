namespace _116.Content.Application.Commerce.Constants;

/// <summary>
/// Contains route path constants for commerce-related API endpoints.
/// Provides centralized string constants for URL segments used in order and payment management routes.
/// </summary>
public static class CommerceRouteConstants
{
    /// <summary>
    /// The base endpoint path for order routes.
    /// Combined with admin prefix: /api/v1/admin/orders.
    /// </summary>
    public const string Orders = "orders";

    /// <summary>
    /// Route segment for order items sub-resource.
    /// Example: /api/v1/admin/orders/{id}/items.
    /// </summary>
    public const string Items = "items";

    /// <summary>
    /// Route segment for item pricing tiers sub-resource.
    /// Example: /api/v1/admin/orders/{id}/items/{itemId}/tiers.
    /// </summary>
    public const string Tiers = "tiers";

    /// <summary>
    /// Route segment for submitting an order.
    /// Example: /api/v1/admin/orders/{id}/submit.
    /// </summary>
    public const string Submit = "submit";

    /// <summary>
    /// Route segment for cancelling an order.
    /// Example: /api/v1/admin/orders/{id}/cancel.
    /// </summary>
    public const string Cancel = "cancel";

    /// <summary>
    /// Route segment for the payment sub-resource.
    /// Example: /api/v1/admin/orders/{id}/payment.
    /// </summary>
    public const string Payment = "payment";

    /// <summary>
    /// Route segment for attaching a payment proof.
    /// Example: /api/v1/admin/orders/{id}/payment/proof.
    /// </summary>
    public const string Proof = "proof";

    /// <summary>
    /// Route segment for verifying a payment.
    /// Example: /api/v1/admin/orders/{id}/payment/verify.
    /// </summary>
    public const string Verify = "verify";

    /// <summary>
    /// Route segment for rejecting a payment.
    /// Example: /api/v1/admin/orders/{id}/payment/reject.
    /// </summary>
    public const string Reject = "reject";

    /// <summary>
    /// Route segment for the pending-payment dashboard view.
    /// Example: /api/v1/admin/orders/pending-payment.
    /// </summary>
    public const string PendingPayment = "pending-payment";
}
