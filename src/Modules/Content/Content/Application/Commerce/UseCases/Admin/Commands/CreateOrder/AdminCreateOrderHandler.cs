using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder.Contracts;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;

/// <summary>
/// Handles the <see cref="AdminCreateOrderCommand" /> to open a new content order.
/// When a package is selected, delegates item/tier creation to the factory.
/// </summary>
/// <param name="customerRepository">Repository for customer data access operations.</param>
/// <param name="packageRepository">Repository for package data access operations.</param>
/// <param name="createOrderFactory">Factory for populating orders from package slots.</param>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminCreateOrderHandler(
    ICustomerRepository customerRepository,
    IPackageRepository packageRepository,
    ICreateOrderFactory createOrderFactory,
    IContentOrderRepository contentOrderRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
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

        if (customer is not null)
        {
            PackageEntity? package = null;

            if (command.PackageId.HasValue)
            {
                package = await packageRepository.GetByIdWithSlotsAsync(
                    id: command.PackageId.Value,
                    cancellationToken: cancellationToken
                );

                if (package is null || !package.IsActive)
                {
                    throw i18n.Package.NotFound(id: command.PackageId.Value);
                }
            }

            var order = ContentOrderEntity.Create(
                id: Guid.NewGuid(),
                customerId: customerId,
                packageId: command.PackageId
            );

            await contentOrderRepository.AddAsync(order: order, ct: cancellationToken);

            int itemCount = 0;

            if (package is not null)
            {
                itemCount = await createOrderFactory.PopulateFromPackageAsync(
                    order: order,
                    package: package,
                    ct: cancellationToken
                );
            }

            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

            var dto = new ContentOrderSummaryDto(
                Id: order.Id,
                Status: order.Status,
                CustomerName: customer.FullName,
                TotalAmountUsd: order.TotalAmountUsd,
                ItemCount: itemCount
            );

            return new AdminCreateOrderResult(Order: dto);
        }

        throw i18n.Customer.NotFound(id: customerId);
    }
}
