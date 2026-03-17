using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment.Contracts;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;

/// <summary>
/// Handles the <see cref="AdminVerifyPaymentCommand" /> to verify an order payment and stamp content promotions.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="orderPaymentFactory">Shared factory for fetching and validating payment records.</param>
/// <param name="verifyPaymentFactory">Factory for the full payment verification and content stamping flow.</param>
public class AdminVerifyPaymentHandler(
    IContentOrderRepository contentOrderRepository,
    IOrderPaymentFactory orderPaymentFactory,
    IVerifyPaymentFactory verifyPaymentFactory
) : ICommandHandler<AdminVerifyPaymentCommand, AdminVerifyPaymentResult>
{
    /// <inheritdoc />
    public async Task<AdminVerifyPaymentResult> Handle(
        AdminVerifyPaymentCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid orderId = Guid.Parse(command.OrderId);

        ContentOrderEntity? order = await contentOrderRepository.GetByIdWithItemsAsync(
            id: orderId,
            ct: cancellationToken
        );

        if (order is null)
        {
            throw ContentOrderErrors.NotFound(id: orderId);
        }

        ContentPaymentEntity payment = await orderPaymentFactory.GetByOrderIdOrThrowAsync(
            orderId: orderId,
            ct: cancellationToken
        );

        await verifyPaymentFactory.VerifyAsync(
            order: order,
            payment: payment,
            adminUserId: command.AdminUserId,
            receiptUrl: command.ReceiptUrl,
            ct: cancellationToken
        );

        return new AdminVerifyPaymentResult(IsSuccess: true);
    }
}
