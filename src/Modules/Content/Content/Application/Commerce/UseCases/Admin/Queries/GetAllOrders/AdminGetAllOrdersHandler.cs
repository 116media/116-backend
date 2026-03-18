using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetAllOrders;

/// <summary>
/// Handles the <see cref="AdminGetAllOrdersQuery" /> to retrieve a paginated list of orders.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetAllOrdersHandler(IContentOrderRepository contentOrderRepository, IMapper mapper)
    : IQueryHandler<AdminGetAllOrdersQuery, AdminGetAllOrdersResult>
{
    /// <inheritdoc />
    public async Task<AdminGetAllOrdersResult> Handle(AdminGetAllOrdersQuery query, CancellationToken cancellationToken)
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (IReadOnlyList<ContentOrderEntity> orders, int totalCount) = await contentOrderRepository.GetAllAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            status: query.Status,
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

        return new AdminGetAllOrdersResult(Orders: paginatedResult);
    }
}
