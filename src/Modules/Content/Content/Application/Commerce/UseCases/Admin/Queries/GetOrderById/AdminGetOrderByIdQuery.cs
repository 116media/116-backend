using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderById;

/// <summary>
/// Query for retrieving a single order with its items, tiers, and payment by identifier.
/// </summary>
/// <param name="Id">The unique identifier of the order.</param>
public record AdminGetOrderByIdQuery(string Id) : IQuery<AdminGetOrderByIdResult>;

/// <summary>
/// Result of the <see cref="AdminGetOrderByIdQuery" /> containing the full order detail.
/// </summary>
/// <param name="Order">The order detail.</param>
public record AdminGetOrderByIdResult(ContentOrderDetailDto Order);
