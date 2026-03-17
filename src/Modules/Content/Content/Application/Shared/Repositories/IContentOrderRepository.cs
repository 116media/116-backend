using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for content order data access operations.
/// Covers all four Commerce entities: orders, items, tiers, and payments.
/// </summary>
public interface IContentOrderRepository : IRepository<ContentOrderEntity>
{
    /// <summary>
    /// Adds a new content order to the repository.
    /// </summary>
    Task AddAsync(ContentOrderEntity order, CancellationToken ct = default);

    /// <summary>
    /// Adds a new order item to the repository.
    /// </summary>
    Task AddItemAsync(ContentOrderItemEntity item, CancellationToken ct = default);

    /// <summary>
    /// Adds a new pricing tier snapshot to the repository.
    /// </summary>
    Task AddItemTierAsync(ContentItemTierEntity tier, CancellationToken ct = default);

    /// <summary>
    /// Adds a new payment record to the repository.
    /// </summary>
    Task AddPaymentAsync(ContentPaymentEntity payment, CancellationToken ct = default);

    /// <summary>
    /// Persists changes to an existing content order.
    /// </summary>
    Task UpdateAsync(ContentOrderEntity order, CancellationToken ct = default);

    /// <summary>
    /// Persists changes to an existing payment record.
    /// </summary>
    Task UpdatePaymentAsync(ContentPaymentEntity payment, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a content order by its unique identifier, including all items, their tiers,
    /// the customer, category, promotion level, and the payment record.
    /// Returns null if not found.
    /// </summary>
    Task<ContentOrderEntity?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a content order by its unique identifier.
    /// Throws a NotFoundException if not found.
    /// </summary>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">Thrown when the order is not found.</exception>
    Task<ContentOrderEntity> GetByIdOrThrowAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a paginated list of content orders with optional filters for status and customer.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="status">Optional filter by order status.</param>
    /// <param name="customerId">Optional filter by customer identifier.</param>
    /// <param name="orderByAscending">When true, orders by <c>CreatedAt</c> ascending (oldest first); defaults to descending.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    Task<(IReadOnlyList<ContentOrderEntity> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        EnumOrderStatus? status,
        Guid? customerId,
        bool orderByAscending = false,
        CancellationToken ct = default
    );

    /// <summary>
    /// Retrieves the payment record for a given order. Returns null if not found.
    /// </summary>
    Task<ContentPaymentEntity?> GetPaymentByOrderIdAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a specific order item that belongs to the given order.
    /// Returns null if not found.
    /// </summary>
    Task<ContentOrderItemEntity?> GetItemByIdAsync(Guid orderId, Guid itemId, CancellationToken ct = default);
}
