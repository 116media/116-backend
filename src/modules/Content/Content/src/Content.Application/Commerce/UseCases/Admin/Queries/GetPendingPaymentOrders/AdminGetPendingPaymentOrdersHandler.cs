using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetPendingPaymentOrders;

/// <summary>
/// Handles the <see cref="AdminGetPendingPaymentOrdersQuery" /> to retrieve orders awaiting payment, oldest-first.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="orderDtoService">Builds order projections with their lookups resolved.</param>
public class AdminGetPendingPaymentOrdersHandler(
    IContentOrderRepository contentOrderRepository,
    IContentOrderDtoService orderDtoService
) : IQueryHandler<AdminGetPendingPaymentOrdersQuery, AdminGetPendingPaymentOrdersResult>
{
    /// <inheritdoc />
    public async Task<AdminGetPendingPaymentOrdersResult> Handle(
        AdminGetPendingPaymentOrdersQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (IReadOnlyList<ContentOrderEntity> orders, int totalCount) = await contentOrderRepository.GetAllAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            status: EnumOrderStatus.PendingPayment,
            customerId: null,
            orderByAscending: true,
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

        return new AdminGetPendingPaymentOrdersResult(Orders: paginatedResult);
    }
}
