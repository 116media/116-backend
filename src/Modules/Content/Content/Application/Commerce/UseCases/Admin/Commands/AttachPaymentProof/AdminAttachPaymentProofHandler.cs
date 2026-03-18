using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof;

/// <summary>
/// Handles the <see cref="AdminAttachPaymentProofCommand" /> to upload a payment proof file (image or PDF)
/// to Cloudinary, persist a <c>FileEntity</c> in <c>core.files</c>, and attach the reference to the
/// order's payment record.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="fileRepository">Repository for uploading and persisting file metadata.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminAttachPaymentProofHandler(
    IContentOrderRepository contentOrderRepository,
    IFileRepository fileRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminAttachPaymentProofCommand, AdminAttachPaymentProofResult>
{
    /// <inheritdoc />
    public async Task<AdminAttachPaymentProofResult> Handle(
        AdminAttachPaymentProofCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid orderId = Guid.Parse(command.OrderId);

        ContentPaymentEntity? payment = await contentOrderRepository.GetPaymentByOrderIdAsync(
            orderId: orderId,
            ct: cancellationToken
        );

        if (payment is null)
        {
            throw ContentOrderErrors.PaymentNotFound(orderId: orderId);
        }

        string mimeType = (command.File.ContentType?.Split(';')[0] ?? string.Empty).Trim().ToLowerInvariant();

        FileEntity proofFile = await fileRepository.UploadAndStoreRawFileAsync(
            file: command.File,
            publicId: command.OrderId,
            folder: "content/payment-proofs",
            originalFileName: command.File.FileName,
            mimeType: mimeType,
            cancellationToken: cancellationToken
        );

        payment.AttachProof(proofFileId: proofFile.Id, paymentMethod: command.PaymentMethod);

        await contentOrderRepository.UpdatePaymentAsync(payment: payment, ct: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        FileDto? proofDto = proofFile.ToFileDto(mapper);
        return new AdminAttachPaymentProofResult(Proof: proofDto!);
    }
}
