using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer;

/// <summary>
/// Handles the <see cref="AdminCreateCustomerCommand" /> to create a new B2B customer.
/// </summary>
/// <param name="customerRepository">Repository for customer data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminCreateCustomerHandler(
    ICustomerRepository customerRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminCreateCustomerCommand, AdminCreateCustomerResult>
{
    /// <inheritdoc />
    public async Task<AdminCreateCustomerResult> Handle(
        AdminCreateCustomerCommand command,
        CancellationToken cancellationToken
    )
    {
        CustomerEntity? existing = await customerRepository.GetByEmailAsync(
            email: command.Email,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            throw CustomerErrors.AlreadyExists(email: command.Email);
        }

        var customer = CustomerEntity.Create(
            id: Guid.NewGuid(),
            fullName: command.FullName,
            email: command.Email,
            phone: command.Phone,
            company: command.Company,
            notes: command.Notes
        );

        await customerRepository.AddAsync(customer: customer, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = customer.ToCustomerDto(mapper);
        return new AdminCreateCustomerResult(Customer: dto);
    }
}
