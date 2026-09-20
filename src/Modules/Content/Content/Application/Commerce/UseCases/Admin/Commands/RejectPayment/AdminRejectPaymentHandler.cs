using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.RejectPayment;

/// <summary>
/// Handles the <see cref="AdminRejectPaymentCommand" /> to reject an order payment.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminRejectPaymentHandler(
    IContentOrderRepository contentOrderRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<AdminRejectPaymentCommand, AdminRejectPaymentResult>
{
    /// <inheritdoc />
    public async Task<AdminRejectPaymentResult> Handle(
        AdminRejectPaymentCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid orderId = Guid.Parse(command.OrderId);

        ContentOrderEntity order = await contentOrderRepository.GetByIdOrThrowAsync(id: orderId, ct: cancellationToken);

        if (order.Payment is null)
        {
            throw i18n.ContentOrder.PaymentNotFound(orderId: orderId);
        }

        order.RejectPayment(notes: command.Notes);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminRejectPaymentResult(IsSuccess: true);
    }
}
