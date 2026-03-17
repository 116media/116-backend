using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.CancelOrder;

/// <summary>
/// Command for cancelling a Draft or PendingPayment order.
/// </summary>
/// <param name="OrderId">The identifier of the order to cancel.</param>
public record AdminCancelOrderCommand(string OrderId) : ICommand<AdminCancelOrderResult>;

/// <summary>
/// The Result returned after successfully cancelling an order.
/// </summary>
/// <param name="IsSuccess">Indicates whether the order was successfully cancelled.</param>
public record AdminCancelOrderResult(bool IsSuccess);
