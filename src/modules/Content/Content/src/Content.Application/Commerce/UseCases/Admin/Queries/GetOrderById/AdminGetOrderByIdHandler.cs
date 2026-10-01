using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderById;

/// <summary>
/// Handles the <see cref="AdminGetOrderByIdQuery" /> to serve an order with its items and payment.
/// </summary>
/// <param name="contentOrderRepository">Repository loading the order with its items.</param>
/// <param name="orderDtoService">Service assembling the order detail.</param>
/// <param name="paymentDtoService">Service assembling the payment with its proof.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminGetOrderByIdHandler(
    IContentOrderRepository contentOrderRepository,
    IContentOrderDtoService orderDtoService,
    IPaymentDtoService paymentDtoService,
    ContentI18n i18n
) : IQueryHandler<AdminGetOrderByIdQuery, AdminGetOrderByIdResult>
{
    /// <inheritdoc />
    public async Task<AdminGetOrderByIdResult> Handle(AdminGetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        ContentOrderEntity? order = await contentOrderRepository.GetByIdWithItemsAsync(
            id: query.Id,
            ct: cancellationToken
        );

        if (order is null)
        {
            throw i18n.ContentOrder.NotFound(id: query.Id);
        }

        ContentOrderDetailDto dto = await orderDtoService.CreateDetailAsync(order, cancellationToken);

        if (order.Payment?.PaymentProofFileId is null)
        {
            return new AdminGetOrderByIdResult(Order: dto);
        }

        dto = dto with { Payment = await paymentDtoService.CreateWithProofAsync(order.Payment, cancellationToken) };
        return new AdminGetOrderByIdResult(Order: dto);
    }
}
