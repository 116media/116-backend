using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;
using Microsoft.AspNetCore.Http;

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
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminAttachPaymentProofHandler(
    IOrderPaymentFactory orderPaymentFactory,
    IFileRepository fileRepository,
    IContentOrderRepository contentOrderRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper,
    ContentI18n i18n
) : ICommandHandler<AdminAttachPaymentProofCommand, AdminAttachPaymentProofResult>
{
    /// <inheritdoc />
    public async Task<AdminAttachPaymentProofResult> Handle(
        AdminAttachPaymentProofCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid orderId = Guid.Parse(command.OrderId);

        await contentOrderRepository.GetByIdOrThrowAsync(id: orderId, ct: cancellationToken);

        ContentPaymentEntity payment = await orderPaymentFactory.GetByOrderIdOrThrowAsync(
            orderId: orderId,
            ct: cancellationToken
        );

        IFormFile file = command.File!;

        string mimeType = file.ContentType.Split(';')[0].Trim().ToLowerInvariant();

        FileEntity proofFile = await fileRepository.UploadAndStoreRawFileAsync(
            file: file,
            mimeType: mimeType,
            publicId: command.OrderId,
            folder: "content/payment-proofs",
            originalFileName: file.FileName,
            cancellationToken: cancellationToken
        );

        payment.AttachProof(proofFileId: proofFile.Id, paymentMethod: command.PaymentMethod, errors: i18n.ContentOrder);

        await contentOrderRepository.UpdatePaymentAsync(payment: payment, ct: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var proofDto = proofFile.ToFileDto(mapper);
        return new AdminAttachPaymentProofResult(Proof: proofDto!);
    }
}
