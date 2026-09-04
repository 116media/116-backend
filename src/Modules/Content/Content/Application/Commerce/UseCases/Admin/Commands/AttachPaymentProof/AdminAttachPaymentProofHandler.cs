using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Services;
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
/// <param name="fileUploadService">Uploads and replaces stored assets.</param>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminAttachPaymentProofHandler(
    IOrderPaymentFactory orderPaymentFactory,
    IFileUploadService fileUploadService,
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

        await contentOrderRepository.GetByIdOrThrowAsync(id: orderId, ct: cancellationToken);

        ContentPaymentEntity payment = await orderPaymentFactory.GetByOrderIdOrThrowAsync(
            orderId: orderId,
            ct: cancellationToken
        );

        IFormFile file = command.File!;

        string mimeType = file.ContentType.Split(';')[0].Trim().ToLowerInvariant();

        FileEntity uploaded = await fileUploadService.UploadRawAsync(
            file: file,
            publicId: command.OrderId,
            folder: "content/payment-proofs",
            originalFileName: file.FileName,
            mimeType: mimeType,
            cancellationToken: cancellationToken
        );

        FileEntity proofFile = await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                FileEntity recorded = await fileUploadService.RecordAsync(file: uploaded, cancellationToken: ct);

                payment.AttachProof(proofFileId: uploaded.Id, paymentMethod: command.PaymentMethod);

                await contentOrderRepository.UpdatePaymentAsync(payment: payment, ct: ct);

                return recorded;
            },
            cancellationToken: cancellationToken
        );

        var proofDto = proofFile.ToFileDto(mapper);
        return new AdminAttachPaymentProofResult(Proof: proofDto!);
    }
}
