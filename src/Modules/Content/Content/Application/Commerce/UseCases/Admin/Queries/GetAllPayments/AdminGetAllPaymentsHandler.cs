using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetAllPayments;

/// <summary>
/// Handles the <see cref="AdminGetAllPaymentsQuery" /> to retrieve a paginated list of payments.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="userLookup">Cross-module service for resolving admin user names.</param>
public class AdminGetAllPaymentsHandler(
    IContentOrderRepository contentOrderRepository,
    IMapper mapper,
    IUserLookupService userLookup
) : IQueryHandler<AdminGetAllPaymentsQuery, AdminGetAllPaymentsResult>
{
    /// <inheritdoc />
    public async Task<AdminGetAllPaymentsResult> Handle(
        AdminGetAllPaymentsQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (IReadOnlyList<ContentPaymentEntity> payments, int totalCount) =
            await contentOrderRepository.GetAllPaymentsAsync(
                page: pageIndex + 1,
                pageSize: pageSize,
                status: query.Status,
                method: query.Method,
                search: query.Search,
                orderByAscending: false,
                ct: cancellationToken
            );

        IReadOnlyList<PaymentSummaryDto> dtoList = await payments.ToPaymentSummaryDtosAsync(
            mapper,
            userLookup,
            cancellationToken
        );

        var paginatedResult = new PaginatedResult<PaymentSummaryDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new AdminGetAllPaymentsResult(Payments: paginatedResult);
    }
}
