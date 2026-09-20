using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IContentOrderRepository" /> for managing content order entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
/// <param name="contentOrderErrors">Content order domain error factory.</param>
public class ContentOrderRepository(ContentDbContext context, ContentOrderErrors contentOrderErrors)
    : ContentRepository<ContentOrderEntity>(context),
        IContentOrderRepository
{
    /// <inheritdoc />
    public async Task AddItemAsync(ContentOrderItemEntity item, CancellationToken ct = default)
    {
        await Context.ContentOrderItems.AddAsync(item, ct);
    }

    /// <inheritdoc />
    public async Task AddItemTierAsync(ContentItemTierEntity tier, CancellationToken ct = default)
    {
        await Context.ContentItemTiers.AddAsync(tier, ct);
    }

    /// <inheritdoc />
    public async Task AddPaymentAsync(ContentPaymentEntity payment, CancellationToken ct = default)
    {
        await Context.ContentPayments.AddAsync(payment, ct);
    }

    /// <inheritdoc />
    public async Task<ContentOrderEntity?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
    {
        return await Context
            .ContentOrders.AsTracking()
            .Where(order => order.Id == id)
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Category)
            .Include(o => o.Items)
                .ThenInclude(i => i.PromotionLevel)
            .Include(o => o.Items)
                .ThenInclude(i => i.Tiers)
                    .ThenInclude(t => t.PricingTier)
            .Include(o => o.Payment)
            .AsSplitQuery()
            .FirstOrDefaultAsync(ct);
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
        IQueryable<ContentOrderEntity> query = Context.ContentOrders;

        if (status.HasValue)
        {
            query = query.Where(order => order.Status == status.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(order => order.CustomerId == customerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search}%";
            query = query.Where(order =>
                EF.Functions.ILike(order.Customer.FullName, pattern)
                || EF.Functions.ILike(order.Customer.Email, pattern)
                || (order.Customer.Company != null && EF.Functions.ILike(order.Customer.Company, pattern))
            );
        }

        query = query.Include(o => o.Customer).Include(o => o.Items);

        int totalCount = await query.CountAsync(ct);

        query = orderByAscending ? query.OrderBy(o => o.CreatedAt) : query.OrderByDescending(o => o.CreatedAt);

        List<ContentOrderEntity> orders = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (orders, totalCount);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<ContentPaymentEntity> Items, int TotalCount)> GetAllPaymentsAsync(
        int page,
        int pageSize,
        EnumPaymentStatus? status,
        EnumPaymentMethod? method,
        string? search = null,
        bool orderByAscending = false,
        CancellationToken ct = default
    )
    {
        IQueryable<ContentPaymentEntity> query = Context.ContentPayments;

        if (status.HasValue)
        {
            query = query.Where(payment => payment.Status == status.Value);
        }

        if (method.HasValue)
        {
            query = query.Where(payment => payment.PaymentMethod == method.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search}%";
            query = query.Where(payment =>
                EF.Functions.ILike(payment.Order.Customer.FullName, pattern)
                || EF.Functions.ILike(payment.Order.Customer.Email, pattern)
                || (
                    payment.Order.Customer.Company != null
                    && EF.Functions.ILike(payment.Order.Customer.Company, pattern)
                )
            );
        }

        query = query.Include(p => p.Order).ThenInclude(o => o.Customer);

        int totalCount = await query.CountAsync(ct);

        query = orderByAscending ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt);

        List<ContentPaymentEntity> payments = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (payments, totalCount);
    }

    /// <inheritdoc />
    public async Task<ContentPaymentEntity?> GetPaymentByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await Context
            .ContentPayments.AsTracking()
            .FirstOrDefaultAsync(payment => payment.OrderId == orderId, ct);
    }

    /// <inheritdoc />
    public async Task<ContentOrderItemEntity?> GetItemByIdAsync(
        Guid orderId,
        Guid itemId,
        CancellationToken ct = default
    )
    {
        // Tracked so the identity map fixes up Tiers from the caller's earlier order load.
        return await Context
            .ContentOrderItems.AsTracking()
            .FirstOrDefaultAsync(item => item.Id == itemId && item.OrderId == orderId, ct);
    }

    /// <inheritdoc />
    public async Task<ContentOrderItemEntity> GetItemByIdOrThrowAsync(
        Guid orderId,
        Guid itemId,
        CancellationToken ct = default
    )
    {
        return await Context
                .ContentOrderItems.AsTracking()
                .FirstOrDefaultAsync(item => item.Id == itemId && item.OrderId == orderId, ct)
            ?? throw contentOrderErrors.ItemNotFound(itemId: itemId);
    }

    /// <inheritdoc />
    public async Task<ContentItemTierEntity?> GetItemTierByIdAsync(
        Guid itemId,
        Guid tierId,
        CancellationToken ct = default
    )
    {
        return await Context.ContentItemTiers.FirstOrDefaultAsync(t => t.OrderItemId == itemId && t.Id == tierId, ct);
    }

    /// <inheritdoc />
    public async Task<ContentItemTierEntity> GetItemTierByIdOrThrowAsync(
        Guid itemId,
        Guid tierId,
        CancellationToken ct = default
    )
    {
        return await Context
                .ContentItemTiers.AsTracking()
                .FirstOrDefaultAsync(t => t.OrderItemId == itemId && t.Id == tierId, ct)
            ?? throw contentOrderErrors.ItemTierNotFound(tierId: tierId);
    }

    /// <inheritdoc />
    public Task RemoveItemAsync(ContentOrderItemEntity item, CancellationToken ct = default)
    {
        Context.ContentOrderItems.Remove(item);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveItemTierAsync(ContentItemTierEntity tier, CancellationToken ct = default)
    {
        Context.ContentItemTiers.Remove(tier);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<ContentOrderEntity?> GetOrderByItemIdAsync(Guid orderItemId, CancellationToken ct = default)
    {
        return await Context.ContentOrders.Where(o => o.Items.Any(i => i.Id == orderItemId)).FirstOrDefaultAsync(ct);
    }
}
