using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.Factories;

/// <summary>
/// Builds order projections, resolving the customer, category, promotion level and pricing tier
/// names in one batch so callers never issue a lookup per row.
/// </summary>
public interface IContentOrderDtoFactory
{
    /// <summary>
    /// Builds the summary projection for one order.
    /// </summary>
    /// <param name="order">The order to project.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The projection.</returns>
    Task<ContentOrderSummaryDto> CreateSummaryAsync(ContentOrderEntity order, CancellationToken ct = default);

    /// <summary>
    /// Builds the summary projections for a list of orders, resolving every customer in one query.
    /// </summary>
    /// <param name="orders">The orders to project.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The projections, in the order supplied.</returns>
    Task<IReadOnlyList<ContentOrderSummaryDto>> CreateManySummariesAsync(
        IReadOnlyList<ContentOrderEntity> orders,
        CancellationToken ct = default
    );

    /// <summary>
    /// Builds the full detail projection for one order, including its items and their tiers.
    /// </summary>
    /// <param name="order">The order to project.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The projection.</returns>
    Task<ContentOrderDetailDto> CreateDetailAsync(ContentOrderEntity order, CancellationToken ct = default);

    /// <summary>
    /// Builds the projection for a single order item.
    /// </summary>
    /// <param name="item">The item to project.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The projection.</returns>
    Task<OrderItemDto> CreateItemAsync(ContentOrderItemEntity item, CancellationToken ct = default);

    /// <summary>
    /// Resolves the customers a set of orders reference, in one query, for a caller assembling
    /// several projections from one batch.
    /// </summary>
    /// <param name="orders">The orders whose customers to resolve.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The customers, keyed by id.</returns>
    Task<IReadOnlyDictionary<Guid, CustomerEntity>> ResolveCustomersAsync(
        IReadOnlyList<ContentOrderEntity> orders,
        CancellationToken ct = default
    );

    /// <summary>
    /// Resolves every row a set of orders names across its items and tiers, in one batch.
    /// </summary>
    /// <param name="orders">The orders to resolve lookups for.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The resolved lookups.</returns>
    Task<OrderLookups> ResolveLookupsAsync(IReadOnlyList<ContentOrderEntity> orders, CancellationToken ct = default);
}
