using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderPayment;

/// <summary>
/// Handles the <see cref="AdminGetOrderPaymentQuery" /> to serve an order's payment with its proof.
/// </summary>
/// <param name="orderPaymentService">Service resolving the order's payment.</param>
/// <param name="contentOrderRepository">Repository asserting the order exists.</param>
/// <param name="paymentDtoService">Service assembling the payment with its proof.</param>
public class AdminGetOrderPaymentHandler(
    IOrderPaymentService orderPaymentService,
    IContentOrderRepository contentOrderRepository,
    IPaymentDtoService paymentDtoService
) : IQueryHandler<AdminGetOrderPaymentQuery, AdminGetOrderPaymentResult>
{
    /// <inheritdoc />
    public async Task<AdminGetOrderPaymentResult> Handle(
        AdminGetOrderPaymentQuery query,
        CancellationToken cancellationToken
    )
    {
        await contentOrderRepository.GetByIdOrThrowAsync(id: query.OrderId, ct: cancellationToken);
        ContentPaymentEntity payment = await orderPaymentService.GetByOrderIdOrThrowAsync(
            orderId: query.OrderId,
            ct: cancellationToken
        );

        var dto = await paymentDtoService.CreateWithProofAsync(payment, cancellationToken);
        return new AdminGetOrderPaymentResult(Payment: dto);
    }
}
