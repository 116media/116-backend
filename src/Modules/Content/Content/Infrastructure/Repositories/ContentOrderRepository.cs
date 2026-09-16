using _116.Content.Application.Commerce.Builders;
using _116.Content.Application.Commerce.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Specifications;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IContentOrderRepository" /> for managing content order entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class ContentOrderRepository(ContentDbContext context)
    : ContentRepository<ContentOrderEntity>(context),
        IContentOrderRepository
{
    /// <inheritdoc />
    public async Task<ContentOrderEntity?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
    {
        var specification = new ContentOrderByIdSpecification(id: id);
        return await QueryTracked().ApplySpecification(specification: specification).FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc />
    protected override IQueryable<ContentOrderEntity> Query()
    {
        return Context
            .ContentOrders.Include(o => o.Items)
                .ThenInclude(i => i.Tiers)
            .Include(o => o.Payment)
            .AsSplitQuery();
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<ContentOrderEntity> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        EnumOrderStatus? status,
        Guid? customerId,
        string? search = null,
        bool orderByAscending = false,
        CancellationToken ct = default
    )
    {
        Specification<ContentOrderEntity>? spec = new ContentOrderQueryBuilder()
            .WithStatus(status: status)
            .WithCustomerId(customerId: customerId)
            .WithSearch(search: search)
            .Build(customers: Context.Customers);

        IQueryable<ContentOrderEntity> query = spec is not null
            ? Context.ContentOrders.ApplySpecification(specification: spec)
            : Context.ContentOrders;

        query = query.Include(o => o.Items);

        int totalCount = await query.CountAsync(ct);

        query = orderByAscending ? query.OrderBy(o => o.CreatedAt) : query.OrderByDescending(o => o.CreatedAt);

        List<ContentOrderEntity> orders = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (orders, totalCount);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<ContentOrderEntity> Items, int TotalCount)> GetOrdersWithPaymentAsync(
        int page,
        int pageSize,
        EnumPaymentStatus? status,
        EnumPaymentMethod? method,
        string? search = null,
        bool orderByAscending = false,
        CancellationToken ct = default
    )
    {
        Specification<ContentOrderEntity>? spec = new ContentPaymentQueryBuilder()
            .WithStatus(status: status)
            .WithMethod(method: method)
            .WithSearch(search: search)
            .Build(customers: Context.Customers);

        IQueryable<ContentOrderEntity> query = Context
            .ContentOrders.ApplySpecification(specification: new OrderHasPaymentSpecification())
            .Include(o => o.Payment);

        if (spec is not null)
        {
            query = query.ApplySpecification(specification: spec);
        }

        int totalCount = await query.CountAsync(ct);

        query = orderByAscending
            ? query.OrderBy(o => o.Payment!.CreatedAt)
            : query.OrderByDescending(o => o.Payment!.CreatedAt);

        List<ContentOrderEntity> orders = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (orders, totalCount);
    }

    /// <inheritdoc />
    public async Task<ContentOrderEntity?> GetOrderByItemIdAsync(Guid orderItemId, CancellationToken ct = default)
    {
        return await Context.ContentOrders.FirstOrDefaultBySpecificationAsync(
            specification: new ContentOrderByItemIdSpecification(orderItemId: orderItemId),
            cancellationToken: ct
        );
    }
}
