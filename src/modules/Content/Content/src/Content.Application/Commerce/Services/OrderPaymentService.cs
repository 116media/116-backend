using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.Contracts.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Commerce.Services;

/// <summary>
/// Service implementation for fetching and validating order payment records.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="contentOrderErrors">Content order domain error service.</param>
/// <param name="fileStorage">Storage contract uploading and recording the proof file.</param>
/// <param name="unitOfWork">Unit of Work committing the proof row and the attachment together.</param>
public class OrderPaymentService(
    IContentOrderRepository contentOrderRepository,
    ContentOrderErrors contentOrderErrors,
    IFileStorageService fileStorage,
    IContentUnitOfWork unitOfWork
) : IOrderPaymentService
{
    /// <inheritdoc />
    public async Task<FileReferenceDto> AttachProofAsync(
        Guid orderId,
        IFormFile file,
        EnumPaymentMethod paymentMethod,
        CancellationToken ct = default
    )
    {
        ContentPaymentEntity payment = await GetByOrderIdOrThrowAsync(orderId: orderId, ct: ct);

        StoredFile uploaded = await fileStorage.UploadAsync(
            file: file,
            publicId: orderId.ToString(),
            folder: "content/payment-proofs",
            kind: EnumStoredFileKind.Raw,
            cancellationToken: ct
        );

        // The file row and the attachment land together, so a proof never dangles unrecorded.
        return await unitOfWork.ExecuteInTransactionAsync(
            async transactionCt =>
            {
                FileReferenceDto recorded = await fileStorage.RecordAsync(
                    file: uploaded,
                    cancellationToken: transactionCt
                );
                payment.AttachProof(proofFileId: uploaded.Reference.Id, paymentMethod: paymentMethod);
                return recorded;
            },
            cancellationToken: ct
        );
    }

    /// <inheritdoc />
    public async Task<ContentPaymentEntity> GetByOrderIdOrThrowAsync(Guid orderId, CancellationToken ct = default)
    {
        ContentOrderEntity? order = await contentOrderRepository.GetByIdWithItemsAsync(id: orderId, ct: ct);

        if (order?.Payment is not null)
        {
            return order.Payment;
        }

        throw contentOrderErrors.PaymentNotFound(orderId: orderId);
    }
}
