using _116.Content.Application.Commerce.Factories;
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
/// <param name="orderPaymentFactory">Shared factory for fetching and validating payment records.</param>
/// <param name="fileRepository">Repository for uploading and persisting file metadata.</param>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminAttachPaymentProofHandler(
    IOrderPaymentFactory orderPaymentFactory,
    IFileRepository fileRepository,
    IContentOrderRepository contentOrderRepository,
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

        ContentPaymentEntity payment = await orderPaymentFactory.GetByOrderIdOrThrowAsync(
            orderId: orderId,
            ct: cancellationToken
        );

        string mimeType = command.File.ContentType.Split(';')[0].Trim().ToLowerInvariant();

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

        var proofDto = proofFile.ToFileDto(mapper);
        return new AdminAttachPaymentProofResult(Proof: proofDto!);
    }
}
