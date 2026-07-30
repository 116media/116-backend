using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.Services;

/// <summary>
/// Sends commerce lifecycle emails to the B2B customer behind an order or a
/// commissioned content record. Customers never log in, so these emails are
/// the platform's only channel to them. Every method is a no-op when no
/// customer is attached (free editorial content) or the recipient cannot be
/// resolved — a notification must never fail an admin operation.
/// </summary>
public interface ICommerceCustomerNotifier
{
    /// <summary>
    /// Sends the invoice-style payment request after an order is submitted.
    /// The order must carry its <c>Customer</c> and <c>Items</c> navigations.
    /// </summary>
    /// <param name="order">The submitted order.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task NotifyOrderInvoiceAsync(ContentOrderEntity order, CancellationToken cancellationToken);

    /// <summary>
    /// Sends the receipt after a payment is verified and the order marked paid.
    /// The order must carry its <c>Customer</c> navigation.
    /// </summary>
    /// <param name="order">The paid order.</param>
    /// <param name="payment">The verified payment carrying amount and receipt URL.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task NotifyPaymentReceiptAsync(
        ContentOrderEntity order,
        ContentPaymentEntity payment,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Tells the customer their payment proof was rejected, quoting the review
    /// notes, so a corrected payment can be sent.
    /// </summary>
    /// <param name="order">The order whose payment was rejected; must carry <c>Customer</c>.</param>
    /// <param name="notes">The reviewer notes explaining the rejection, or <c>null</c> when none were provided.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task NotifyPaymentRejectedAsync(ContentOrderEntity order, string? notes, CancellationToken cancellationToken);

    /// <summary>
    /// Tells the customer their order was cancelled. Resolves the customer by
    /// id since the cancel path loads the bare order.
    /// </summary>
    /// <param name="order">The cancelled order.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task NotifyOrderCancelledAsync(ContentOrderEntity order, CancellationToken cancellationToken);

    /// <summary>
    /// Tells the paying customer their promoted placement was removed early
    /// and why. Skips silently when the content has no customer.
    /// </summary>
    /// <param name="customerId">The customer behind the placement, if any.</param>
    /// <param name="contentTitle">The promoted content's display title.</param>
    /// <param name="reason">The admin-provided removal reason.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task NotifyPromotionRemovedAsync(
        Guid? customerId,
        string contentTitle,
        string reason,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Tells the paying customer their commissioned content is live. Skips
    /// silently for free editorial content.
    /// </summary>
    /// <param name="customerId">The customer behind the content, if any.</param>
    /// <param name="contentTitle">The content's display title.</param>
    /// <param name="publicUrl">The public frontend URL of the published content.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task NotifyContentPublishedAsync(
        Guid? customerId,
        string contentTitle,
        string publicUrl,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Tells the paying customer their commissioned content failed editorial
    /// review and why. Skips silently for free editorial content.
    /// </summary>
    /// <param name="customerId">The customer behind the content, if any.</param>
    /// <param name="contentTitle">The content's display title.</param>
    /// <param name="reason">The captured rejection reason.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task NotifyContentRejectedAsync(
        Guid? customerId,
        string contentTitle,
        string reason,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Tells the customer the shoot date for their pre-booked video production.
    /// </summary>
    /// <param name="customerId">The customer behind the production, if any.</param>
    /// <param name="contentTitle">The video's display title.</param>
    /// <param name="shootDate">The scheduled shoot date.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task NotifyShootScheduledAsync(
        Guid? customerId,
        string contentTitle,
        DateTime shootDate,
        CancellationToken cancellationToken
    );
}
