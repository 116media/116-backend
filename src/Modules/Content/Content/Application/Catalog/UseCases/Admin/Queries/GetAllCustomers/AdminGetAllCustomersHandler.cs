using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCustomers;

/// <summary>
/// Handles the <see cref="AdminGetAllCustomersQuery" /> to retrieve a paginated list of customers.
/// </summary>
/// <param name="customerRepository">Repository for customer data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetAllCustomersHandler(ICustomerRepository customerRepository, IMapper mapper)
    : IQueryHandler<AdminGetAllCustomersQuery, AdminGetAllCustomersResult>
{
    /// <inheritdoc />
    public async Task<AdminGetAllCustomersResult> Handle(
        AdminGetAllCustomersQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (List<CustomerEntity> customers, int totalCount) = await customerRepository.GetAllAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            cancellationToken: cancellationToken
        );

        List<CustomerDto> dtoList = customers.Select(c => c.ToCustomerDto(mapper)).ToList();

        var paginatedResult = new PaginatedResult<CustomerDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new AdminGetAllCustomersResult(Customers: paginatedResult);
    }
}
