using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderPayment;

/// <summary>
/// Handles the <see cref="AdminGetOrderPaymentQuery" /> to retrieve the payment record for an order.
/// </summary>
/// <param name="orderPaymentFactory">Shared factory for fetching and validating payment records.</param>
/// <param name="fileRepository">Repository for resolving payment proof file metadata.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetOrderPaymentHandler(
    IOrderPaymentFactory orderPaymentFactory,
    IFileRepository fileRepository,
    IMapper mapper
) : IQueryHandler<AdminGetOrderPaymentQuery, AdminGetOrderPaymentResult>
{
    /// <inheritdoc />
    public async Task<AdminGetOrderPaymentResult> Handle(
        AdminGetOrderPaymentQuery query,
        CancellationToken cancellationToken
    )
    {
        ContentPaymentEntity payment = await orderPaymentFactory.GetByOrderIdOrThrowAsync(
            orderId: query.OrderId,
            ct: cancellationToken
        );

        FileEntity? proofFile = payment.PaymentProofFileId.HasValue
            ? await fileRepository.GetByIdAsync(payment.PaymentProofFileId.Value, cancellationToken)
            : null;

        var proofDto = proofFile.ToFileDto(mapper);
        var dto = payment.ToPaymentDto(mapper, proofDto);

        return new AdminGetOrderPaymentResult(Payment: dto);
    }
}
