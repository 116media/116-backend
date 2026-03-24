using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;

/// <summary>
/// Handles the <see cref="AdminCreateOrderCommand" /> to open a new content order.
/// </summary>
/// <param name="customerRepository">Repository for customer data access operations.</param>
/// <param name="packageRepository">Repository for package data access operations.</param>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminCreateOrderHandler(
    ICustomerRepository customerRepository,
    IPackageRepository packageRepository,
    IContentOrderRepository contentOrderRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminCreateOrderCommand, AdminCreateOrderResult>
{
    /// <inheritdoc />
    public async Task<AdminCreateOrderResult> Handle(
        AdminCreateOrderCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid customerId = Guid.Parse(command.CustomerId);

        CustomerEntity? customer = await customerRepository.GetByIdAsync(
            id: customerId,
            cancellationToken: cancellationToken
        );

        if (customer is null)
        {
            throw CustomerErrors.NotFound(id: customerId);
        }

        if (command.PackageId.HasValue)
        {
            PackageEntity? package = await packageRepository.GetByIdWithSlotsAsync(
                id: command.PackageId.Value,
                cancellationToken: cancellationToken
            );

            if (package is null || !package.IsActive)
            {
                throw PackageErrors.NotFound(id: command.PackageId.Value);
            }
        }

        var order = ContentOrderEntity.Create(id: Guid.NewGuid(), customerId: customerId, packageId: command.PackageId);

        await contentOrderRepository.AddAsync(order: order, ct: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = new ContentOrderSummaryDto(
            Id: order.Id,
            CustomerName: customer.FullName,
            Status: order.Status,
            TotalAmountUsd: order.TotalAmountUsd,
            ItemCount: 0
        );

        return new AdminCreateOrderResult(Order: dto);
    }
}
