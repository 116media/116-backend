using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Contracts.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof;

/// <summary>
/// Handles the <see cref="AdminAttachPaymentProofCommand" /> to upload a payment proof file (image or PDF)
/// to Cloudinary, persist a <c>FileReferenceDto</c> in <c>core.files</c>, and attach the reference to the
/// order's payment record.
/// </summary>
/// <param name="orderPaymentFactory">Shared factory for fetching and validating payment records.</param>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminAttachPaymentProofHandler(
    IOrderPaymentFactory orderPaymentFactory,
    IFileStorageService fileStorage,
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

        StoredFile uploaded = await fileStorage.UploadAsync(
            file: file,
            publicId: command.OrderId,
            folder: "content/payment-proofs",
            kind: EnumStoredFileKind.Raw,
            cancellationToken: cancellationToken
        );

        FileReferenceDto proofFile = await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                FileReferenceDto recorded = await fileStorage.RecordAsync(file: uploaded, cancellationToken: ct);

                payment.AttachProof(proofFileId: uploaded.Reference.Id, paymentMethod: command.PaymentMethod);

                await contentOrderRepository.UpdatePaymentAsync(payment: payment, ct: ct);

                return recorded;
            },
            cancellationToken: cancellationToken
        );

        var proofDto = proofFile.ToFileDto(mapper);
        return new AdminAttachPaymentProofResult(Proof: proofDto!);
    }
}
