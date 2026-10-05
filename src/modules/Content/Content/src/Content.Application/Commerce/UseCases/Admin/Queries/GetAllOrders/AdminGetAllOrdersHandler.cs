using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetAllOrders;

/// <summary>
/// Handles the <see cref="AdminGetAllOrdersQuery" /> to retrieve a paginated list of orders.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="orderDtoService">Builds order projections with their lookups resolved.</param>
public class AdminGetAllOrdersHandler(
    IContentOrderRepository contentOrderRepository,
    IContentOrderDtoService orderDtoService
) : IQueryHandler<AdminGetAllOrdersQuery, AdminGetAllOrdersResult>
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
            search: query.Search,
            orderByAscending: false,
            ct: cancellationToken
        );

        IReadOnlyList<ContentOrderSummaryDto> dtoList = await orderDtoService.CreateManySummariesAsync(
            orders,
            cancellationToken
        );

        var paginatedResult = new PaginatedResult<ContentOrderSummaryDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new AdminGetAllOrdersResult(Orders: paginatedResult);
    }
}
