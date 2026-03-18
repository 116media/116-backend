using _116.Content.Application.Commerce.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IContentOrderRepository" /> for managing content order entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class ContentOrderRepository(ContentDbContext context) : IContentOrderRepository
{
    /// <inheritdoc />
    public async Task AddAsync(ContentOrderEntity order, CancellationToken ct = default)
    {
        await context.ContentOrders.AddAsync(order, ct);
    }

    /// <inheritdoc />
    public async Task AddItemAsync(ContentOrderItemEntity item, CancellationToken ct = default)
    {
        await context.ContentOrderItems.AddAsync(item, ct);
    }

    /// <inheritdoc />
    public async Task AddItemTierAsync(ContentItemTierEntity tier, CancellationToken ct = default)
    {
        await context.ContentItemTiers.AddAsync(tier, ct);
    }

    /// <inheritdoc />
    public async Task AddPaymentAsync(ContentPaymentEntity payment, CancellationToken ct = default)
    {
        await context.ContentPayments.AddAsync(payment, ct);
    }

    /// <inheritdoc />
    public Task UpdateAsync(ContentOrderEntity order, CancellationToken ct = default)
    {
        context.ContentOrders.Update(order);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdatePaymentAsync(ContentPaymentEntity payment, CancellationToken ct = default)
    {
        context.ContentPayments.Update(payment);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<ContentOrderEntity?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
    {
        var specification = new ContentOrderByIdSpecification(id: id);
        return await context
            .ContentOrders.ApplySpecification(specification: specification)
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Category)
            .Include(o => o.Items)
                .ThenInclude(i => i.PromotionLevel)
            .Include(o => o.Items)
                .ThenInclude(i => i.Tiers)
                    .ThenInclude(t => t.PricingTier)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc />
    public async Task<ContentOrderEntity> GetByIdOrThrowAsync(Guid id, CancellationToken ct = default)
    {
        var specification = new ContentOrderByIdSpecification(id: id);
        return await context
            .ContentOrders.ApplySpecification(specification: specification)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: ct);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<ContentOrderEntity> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        EnumOrderStatus? status,
        Guid? customerId,
        bool orderByAscending = false,
        CancellationToken ct = default
    )
    {
        IQueryable<ContentOrderEntity> query = context
            .ContentOrders.Include(o => o.Customer)
            .Include(o => o.Items)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.ApplySpecification(new ContentOrderByStatusSpecification(status: status.Value));
        }

        if (customerId.HasValue)
        {
            query = query.ApplySpecification(new ContentOrderByCustomerIdSpecification(customerId: customerId.Value));
        }

        int totalCount = await query.CountAsync(ct);

        query = orderByAscending ? query.OrderBy(o => o.CreatedAt) : query.OrderByDescending(o => o.CreatedAt);

        List<ContentOrderEntity> orders = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (orders, totalCount);
    }

    /// <inheritdoc />
    public async Task<ContentPaymentEntity?> GetPaymentByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        var specification = new ContentPaymentByOrderIdSpecification(orderId: orderId);
        return await context.ContentPayments.ApplySpecification(specification: specification).FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc />
    public async Task<ContentOrderItemEntity?> GetItemByIdAsync(
        Guid orderId,
        Guid itemId,
        CancellationToken ct = default
    )
    {
        var specification = new ContentOrderItemByIdAndOrderIdSpecification(orderId: orderId, itemId: itemId);
        return await context.ContentOrderItems.ApplySpecification(specification: specification).FirstOrDefaultAsync(ct);
    }
}
