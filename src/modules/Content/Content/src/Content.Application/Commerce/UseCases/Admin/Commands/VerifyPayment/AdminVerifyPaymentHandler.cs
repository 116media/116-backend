using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;

/// <summary>
/// Handles the <see cref="AdminVerifyPaymentCommand" /> to verify an order payment and stamp content promotions.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="orderPaymentService">Shared service for fetching and validating payment records.</param>
/// <param name="verifyPaymentService">Service for the full payment verification and content stamping flow.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminVerifyPaymentHandler(
    IContentOrderRepository contentOrderRepository,
    IOrderPaymentService orderPaymentService,
    IVerifyPaymentService verifyPaymentService,
    ContentI18n i18n
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

        if (order is not null)
        {
            ContentPaymentEntity payment = await orderPaymentService.GetByOrderIdOrThrowAsync(
                orderId: orderId,
                ct: cancellationToken
            );

            await verifyPaymentService.VerifyAsync(
                order: order,
                payment: payment,
                adminUserId: command.AdminUserId,
                receiptUrl: command.ReceiptUrl,
                ct: cancellationToken
            );

            return new AdminVerifyPaymentResult(IsSuccess: true);
        }

        throw i18n.ContentOrder.NotFound(id: orderId);
    }
}
