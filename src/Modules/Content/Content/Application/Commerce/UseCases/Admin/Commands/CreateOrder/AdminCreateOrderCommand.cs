using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;

/// <summary>
/// Command for opening a new content order for a B2B client.
/// </summary>
/// <param name="CustomerId">The B2B customer placing the order.</param>
/// <param name="PackageId">An optional pre-configured package to apply to this order.</param>
public record AdminCreateOrderCommand(string CustomerId, Guid? PackageId) : ICommand<AdminCreateOrderResult>;

/// <summary>
/// Result of the <see cref="AdminCreateOrderCommand" /> containing the created order summary.
/// </summary>
/// <param name="Order">The created order summary.</param>
public record AdminCreateOrderResult(ContentOrderSummaryDto Order);
