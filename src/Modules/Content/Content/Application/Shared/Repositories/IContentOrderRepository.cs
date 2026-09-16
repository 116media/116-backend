using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for content order data access operations.
/// Covers all four Commerce entities: orders, items, tiers, and payments.
/// </summary>
public interface IContentOrderRepository
{
    /// <summary>
    /// Adds a new content order to the repository.
    /// </summary>
    Task AddAsync(ContentOrderEntity order, CancellationToken ct = default);

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
    /// Retrieves a paginated list of content orders with optional filters for status, customer, and search.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="status">Optional filter by order status.</param>
    /// <param name="customerId">Optional filter by customer identifier.</param>
    /// <param name="search">Optional search term matching customer name, email, or company.</param>
    /// <param name="orderByAscending">When true, orders by <c>CreatedAt</c> ascending (oldest first); defaults to descending.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    Task<(IReadOnlyList<ContentOrderEntity> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        EnumOrderStatus? status,
        Guid? customerId,
        string? search = null,
        bool orderByAscending = false,
        CancellationToken ct = default
    );

    /// <summary>
    /// Retrieves a paginated list of the orders that carry a payment, each with its payment
    /// loaded — the root the admin payments listing pages over.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="status">Optional filter by payment verification status.</param>
    /// <param name="method">Optional filter by payment method.</param>
    /// <param name="search">Optional search term matching customer name, email, or company.</param>
    /// <param name="orderByAscending">When true, orders by <c>CreatedAt</c> ascending; defaults to descending.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    Task<(IReadOnlyList<ContentOrderEntity> Items, int TotalCount)> GetOrdersWithPaymentAsync(
        int page,
        int pageSize,
        EnumPaymentStatus? status,
        EnumPaymentMethod? method,
        string? search = null,
        bool orderByAscending = false,
        CancellationToken ct = default
    );

    /// <summary>
    /// Retrieves the order that owns the given order item. Returns null if not found.
    /// Used to check order payment status before transitioning content status on submit.
    /// </summary>
    Task<ContentOrderEntity?> GetOrderByItemIdAsync(Guid orderItemId, CancellationToken ct = default);
}
