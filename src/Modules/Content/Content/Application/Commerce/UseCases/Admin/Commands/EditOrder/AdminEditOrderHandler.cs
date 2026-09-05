using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder;

/// <summary>
/// Handles the <see cref="AdminEditOrderCommand" /> to edit a draft content order.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="customerRepository">Repository for customer data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapper for entity-to-DTO conversion.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminEditOrderHandler(
    IContentOrderRepository contentOrderRepository,
    ICustomerRepository customerRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper,
    ContentI18n i18n
) : ICommandHandler<AdminEditOrderCommand, AdminEditOrderResult>
{
    /// <inheritdoc />
    public async Task<AdminEditOrderResult> Handle(AdminEditOrderCommand command, CancellationToken cancellationToken)
    {
        Guid orderId = Guid.Parse(command.OrderId);

        ContentOrderEntity order =
            await contentOrderRepository.GetByIdWithItemsAsync(id: orderId, ct: cancellationToken)
            ?? throw i18n.ContentOrder.NotFound(id: orderId);

        Guid? newCustomerId = command.CustomerId is not null ? Guid.Parse(command.CustomerId) : null;

        if (newCustomerId.HasValue)
        {
            await customerRepository.GetByIdOrThrowAsync(id: newCustomerId.Value, cancellationToken: cancellationToken);
        }

        order.Update(customerId: newCustomerId, packageId: command.PackageId);

        await contentOrderRepository.UpdateAsync(order: order, ct: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        ContentOrderEntity updated =
            await contentOrderRepository.GetByIdWithItemsAsync(id: orderId, ct: cancellationToken)
            ?? throw i18n.ContentOrder.NotFound(id: orderId);

        var dto = updated.ToContentOrderSummaryDto(mapper);

        return new AdminEditOrderResult(Order: dto);
    }
}
