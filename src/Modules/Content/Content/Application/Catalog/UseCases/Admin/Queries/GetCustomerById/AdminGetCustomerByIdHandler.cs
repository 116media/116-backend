using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCustomerById;

/// <summary>
/// Handles the <see cref="AdminGetCustomerByIdQuery" /> to retrieve a customer by its identifier.
/// </summary>
/// <param name="customerRepository">Repository for customer data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetCustomerByIdHandler(ICustomerRepository customerRepository, IMapper mapper)
    : IQueryHandler<AdminGetCustomerByIdQuery, AdminGetCustomerByIdResult>
{
    /// <inheritdoc />
    public async Task<AdminGetCustomerByIdResult> Handle(
        AdminGetCustomerByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        CustomerEntity customer = await customerRepository.GetByIdOrThrowAsync(
            id: query.Id,
            cancellationToken: cancellationToken
        );

        var dto = customer.ToCustomerDto(mapper);
        return new AdminGetCustomerByIdResult(Customer: dto);
    }
}
