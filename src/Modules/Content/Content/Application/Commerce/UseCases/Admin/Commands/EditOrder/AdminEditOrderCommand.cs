using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder;

/// <summary>
/// Command for editing a draft content order's customer or package assignment.
/// </summary>
/// <param name="OrderId">The identifier of the order to edit.</param>
/// <param name="CustomerId">The new customer ID, or null to keep the current one.</param>
/// <param name="PackageId">The new package ID, or null to clear it.</param>
public record AdminEditOrderCommand(string OrderId, string? CustomerId, Guid? PackageId)
    : ICommand<AdminEditOrderResult>;

/// <summary>
/// Result of the <see cref="AdminEditOrderCommand" /> containing the updated order summary.
/// </summary>
/// <param name="Order">The updated order summary.</param>
public record AdminEditOrderResult(ContentOrderSummaryDto Order);
