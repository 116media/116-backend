using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.RejectPayment;

/// <summary>
/// Handles the <see cref="AdminRejectPaymentCommand" /> to reject an order payment.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminRejectPaymentHandler(IContentOrderRepository contentOrderRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<AdminRejectPaymentCommand, AdminRejectPaymentResult>
{
    /// <inheritdoc />
    public async Task<AdminRejectPaymentResult> Handle(
        AdminRejectPaymentCommand command,
        CancellationToken cancellationToken = default
    )
    {
        Guid orderId = Guid.Parse(command.OrderId);

        ContentOrderEntity order = await contentOrderRepository.GetByIdOrThrowAsync(id: orderId, ct: cancellationToken);

        if (order.Payment is null)
        {
            throw ContentOrderErrors.PaymentNotFound(orderId: orderId);
        }

        order.RejectPayment(notes: command.Notes);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminRejectPaymentResult(IsSuccess: true);
    }
}
