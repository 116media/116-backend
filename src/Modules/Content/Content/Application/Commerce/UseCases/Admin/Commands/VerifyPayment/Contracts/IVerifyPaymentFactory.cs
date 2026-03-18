using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment.Contracts;

/// <summary>
/// Factory for handling the full payment verification flow, including
/// stamping social boost and featured promotion on linked articles/videos.
/// </summary>
public interface IVerifyPaymentFactory
{
    /// <summary>
    /// Verifies the payment, marks the order as paid, and stamps promotion
    /// attributes on linked editorial content for each order item.
    /// </summary>
    Task VerifyAsync(
        ContentOrderEntity order,
        ContentPaymentEntity payment,
        Guid adminUserId,
        string receiptUrl,
        CancellationToken ct
    );
}
