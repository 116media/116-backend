using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Storage.Contracts.Application.DTOs;
using MapsterMapper;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof;

/// <summary>
/// Handles the <see cref="AdminAttachPaymentProofCommand" /> to attach a proof file to an order's payment.
/// </summary>
/// <param name="orderPaymentService">Service uploading and attaching the proof.</param>
/// <param name="contentOrderRepository">Repository asserting the order exists.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminAttachPaymentProofHandler(
    IOrderPaymentService orderPaymentService,
    IContentOrderRepository contentOrderRepository,
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

        FileReferenceDto proofFile = await orderPaymentService.AttachProofAsync(
            orderId: orderId,
            file: command.File!,
            paymentMethod: command.PaymentMethod,
            ct: cancellationToken
        );

        var proofDto = proofFile.ToFileDto(mapper);
        return new AdminAttachPaymentProofResult(Proof: proofDto!);
    }
}
