using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCustomer;

/// <summary>
/// Handles the <see cref="AdminUpdateCustomerCommand" /> to update an existing customer's contact information.
/// </summary>
/// <param name="customerRepository">Repository for customer data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminUpdateCustomerHandler(
    ICustomerRepository customerRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminUpdateCustomerCommand, AdminUpdateCustomerResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateCustomerResult> Handle(
        AdminUpdateCustomerCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        CustomerEntity customer = await customerRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        customer.Update(
            fullName: command.FullName,
            phone: command.Phone,
            company: command.Company,
            notes: command.Notes
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = customer.ToCustomerDto(mapper);
        return new AdminUpdateCustomerResult(Customer: dto);
    }
}
