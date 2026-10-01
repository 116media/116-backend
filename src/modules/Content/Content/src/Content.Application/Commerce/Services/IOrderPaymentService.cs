using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Storage.Contracts.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Commerce.Services;

/// <summary>
/// Shared service for fetching and validating order payment records.
/// Centralizes the common "fetch payment or throw" logic used across
/// AttachPaymentProof, VerifyPayment, RejectPayment, and GetOrderPayment.
/// </summary>
public interface IOrderPaymentService
{
    /// <summary>
    /// Retrieves the payment record for an order, throwing if not found.
    /// </summary>
    Task<ContentPaymentEntity> GetByOrderIdOrThrowAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>
    /// Uploads a proof file and attaches it to the order's payment, committing the file row and the
    /// attachment together.
    /// </summary>
    /// <param name="orderId">The order whose payment receives the proof.</param>
    /// <param name="file">The uploaded proof.</param>
    /// <param name="paymentMethod">The method the payer used.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    /// <returns>The recorded proof file.</returns>
    Task<FileReferenceDto> AttachProofAsync(
        Guid orderId,
        IFormFile file,
        EnumPaymentMethod paymentMethod,
        CancellationToken ct = default
    );
}
