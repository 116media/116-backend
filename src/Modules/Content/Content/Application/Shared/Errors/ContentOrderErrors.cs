using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Content order domain error factory providing simple, readable exception creation.
/// Usage: ContentOrderErrors.NotFound(id) or ContentOrderErrors.AlreadySubmitted()
/// </summary>
public static class ContentOrderErrors
{
    /// <summary>
    /// Throws when a content order is not found by its identifier.
    /// </summary>
    public static NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("ContentOrder", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a content order item is not found by its identifier.
    /// </summary>
    public static NotFoundException ItemNotFound(Guid itemId)
    {
        return new NotFoundException("ContentOrderItem", "id", keyValue: itemId);
    }

    /// <summary>
    /// Throws when a payment record is not found for the given order.
    /// </summary>
    public static NotFoundException PaymentNotFound(Guid orderId)
    {
        return new NotFoundException("ContentPayment", "orderId", keyValue: orderId);
    }

    /// <summary>
    /// Throws when the order has already been submitted and cannot be submitted again.
    /// </summary>
    public static ConflictException AlreadySubmitted()
    {
        return new ConflictException(ContentOrderErrorMessage.AlreadySubmitted());
    }

    /// <summary>
    /// Throws when the order has already been paid and cannot be marked as paid again.
    /// </summary>
    public static ConflictException AlreadyPaid()
    {
        return new ConflictException(ContentOrderErrorMessage.AlreadyPaid());
    }

    /// <summary>
    /// Throws when the order has already been cancelled.
    /// </summary>
    public static ConflictException AlreadyCancelled()
    {
        return new ConflictException(ContentOrderErrorMessage.AlreadyCancelled());
    }

    /// <summary>
    /// Throws when an attempt is made to cancel an order that has already been paid.
    /// </summary>
    public static BadRequestException CannotCancelPaidOrder()
    {
        return new BadRequestException(ContentOrderErrorMessage.CannotCancelPaidOrder());
    }

    /// <summary>
    /// Throws when an attempt is made to add an item to an order that is not in Draft status.
    /// </summary>
    public static BadRequestException CannotAddItemToNonDraftOrder()
    {
        return new BadRequestException(ContentOrderErrorMessage.CannotAddItemToNonDraftOrder());
    }

    /// <summary>
    /// Throws when an order is submitted without at least one item that has at least one pricing tier.
    /// </summary>
    public static BadRequestException MustHaveAtLeastOneItemWithTier()
    {
        return new BadRequestException(ContentOrderErrorMessage.MustHaveAtLeastOneItemWithTier());
    }

    /// <summary>
    /// Throws when the payment has already been verified and cannot be verified again.
    /// </summary>
    public static ConflictException PaymentAlreadyVerified()
    {
        return new ConflictException(ContentOrderErrorMessage.PaymentAlreadyVerified());
    }

    /// <summary>
    /// Throws when the payment has already been rejected and cannot be rejected again.
    /// </summary>
    public static ConflictException PaymentAlreadyRejected()
    {
        return new ConflictException(ContentOrderErrorMessage.PaymentAlreadyRejected());
    }
}
