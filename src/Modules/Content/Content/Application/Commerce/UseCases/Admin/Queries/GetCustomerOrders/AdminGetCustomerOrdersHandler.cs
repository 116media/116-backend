using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetCustomerOrders;

/// <summary>
/// Handles the <see cref="AdminGetCustomerOrdersQuery" /> to retrieve paginated orders for a specific customer.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetCustomerOrdersHandler(IContentOrderRepository contentOrderRepository, IMapper mapper)
    : IQueryHandler<AdminGetCustomerOrdersQuery, AdminGetCustomerOrdersResult>
{
    /// <inheritdoc />
    public async Task<AdminGetCustomerOrdersResult> Handle(
        AdminGetCustomerOrdersQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (IReadOnlyList<ContentOrderEntity> orders, int totalCount) = await contentOrderRepository.GetAllAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            status: null,
            customerId: query.CustomerId,
            orderByAscending: false,
            ct: cancellationToken
        );

        IReadOnlyList<ContentOrderSummaryDto> dtoList = orders.ToContentOrderSummaryDtos(mapper);

        var paginatedResult = new PaginatedResult<ContentOrderSummaryDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new AdminGetCustomerOrdersResult(Orders: paginatedResult);
    }
}
