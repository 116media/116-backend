using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Content order domain error factory providing simple, readable exception creation.
/// Usage: ContentOrderErrors.NotFound(id) or ContentOrderErrors.AlreadySubmitted()
/// </summary>
public class ContentOrderErrors(ContentOrderErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validator extensions.
    /// </summary>
    public ContentOrderErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when a content order is not found by its identifier.
    /// </summary>
    public NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("ContentOrder", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a content order item is not found by its identifier.
    /// </summary>
    public NotFoundException ItemNotFound(Guid itemId)
    {
        return new NotFoundException("ContentOrderItem", "id", keyValue: itemId);
    }

    /// <summary>
    /// Throws when a pricing tier is already attached to the order item.
    /// </summary>
    public ConflictException TierAlreadyAttached()
    {
        return new ConflictException(i18n.TierAlreadyAttached());
    }

    /// <summary>
    /// Throws when a payment record is not found for the given order.
    /// </summary>
    public NotFoundException PaymentNotFound(Guid orderId)
    {
        return new NotFoundException("ContentPayment", "orderId", keyValue: orderId);
    }

    /// <summary>
    /// Throws when the order has already been submitted and cannot be submitted again.
    /// </summary>
    public ConflictException AlreadySubmitted()
    {
        return new ConflictException(i18n.AlreadySubmitted());
    }

    /// <summary>
    /// Throws when the order has already been paid and cannot be marked as paid again.
    /// </summary>
    public ConflictException AlreadyPaid()
    {
        return new ConflictException(i18n.AlreadyPaid());
    }

    /// <summary>
    /// Throws when the order has already been cancelled.
    /// </summary>
    public ConflictException AlreadyCancelled()
    {
        return new ConflictException(i18n.AlreadyCancelled());
    }

    /// <summary>
    /// Throws when an attempt is made to cancel an order that has already been paid.
    /// </summary>
    public BadRequestException CannotCancelPaidOrder()
    {
        return new BadRequestException(i18n.CannotCancelPaidOrder());
    }

    /// <summary>
    /// Throws when an attempt is made to add an item to an order that is not in Draft status.
    /// </summary>
    public BadRequestException CannotAddItemToNonDraftOrder()
    {
        return new BadRequestException(i18n.CannotAddItemToNonDraftOrder());
    }

    /// <summary>
    /// Throws when an order is submitted without at least one item that has at least one pricing tier.
    /// </summary>
    public BadRequestException MustHaveAtLeastOneItemWithTier()
    {
        return new BadRequestException(i18n.MustHaveAtLeastOneItemWithTier());
    }

    /// <summary>
    /// Throws when a content item tier is not found by its identifier.
    /// </summary>
    public NotFoundException ItemTierNotFound(Guid tierId)
    {
        return new NotFoundException("ContentItemTier", "id", keyValue: tierId);
    }

    /// <summary>
    /// Throws when the payment has already been verified and cannot be verified again.
    /// </summary>
    public ConflictException PaymentAlreadyVerified()
    {
        return new ConflictException(i18n.PaymentAlreadyVerified());
    }

    /// <summary>
    /// Throws when the payment has already been rejected and cannot be rejected again.
    /// </summary>
    public ConflictException PaymentAlreadyRejected()
    {
        return new ConflictException(i18n.PaymentAlreadyRejected());
    }

    /// <summary>
    /// Throws when the promotion duration of a level purchased by an order item
    /// cannot be resolved, so the item's promotion window cannot be computed.
    /// </summary>
    public BadRequestException PromotionDurationUnavailable()
    {
        return new BadRequestException(i18n.PromotionDurationUnavailable());
    }
}
