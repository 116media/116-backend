using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder;

/// <summary>
/// Command for submitting a Draft order, transitioning it to PendingPayment and creating the payment record.
/// </summary>
/// <param name="OrderId">The identifier of the order to submit.</param>
public record AdminSubmitOrderCommand(string OrderId) : ICommand<AdminSubmitOrderResult>;

/// <summary>
/// Result returned after successfully submitting an order.
/// </summary>
/// <param name="IsSuccess">Indicates whether the order was successfully submitted.</param>
public record AdminSubmitOrderResult(bool IsSuccess);
