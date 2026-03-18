using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.RejectPayment;

/// <summary>
/// Handles the <see cref="AdminRejectPaymentCommand" /> to reject an order payment.
/// </summary>
/// <param name="orderPaymentFactory">Shared factory for fetching and validating payment records.</param>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminRejectPaymentHandler(
    IOrderPaymentFactory orderPaymentFactory,
    IContentOrderRepository contentOrderRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminRejectPaymentCommand, AdminRejectPaymentResult>
{
    /// <inheritdoc />
    public async Task<AdminRejectPaymentResult> Handle(
        AdminRejectPaymentCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid orderId = Guid.Parse(command.OrderId);

        ContentPaymentEntity payment = await orderPaymentFactory.GetByOrderIdOrThrowAsync(
            orderId: orderId,
            ct: cancellationToken
        );

        payment.Reject(notes: command.Notes);

        await contentOrderRepository.UpdatePaymentAsync(payment: payment, ct: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminRejectPaymentResult(IsSuccess: true);
    }
}
