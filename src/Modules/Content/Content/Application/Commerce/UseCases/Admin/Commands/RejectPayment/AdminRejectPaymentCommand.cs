using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.RejectPayment;

/// <summary>
/// Command for rejecting an order payment with optional notes.
/// </summary>
/// <param name="OrderId">The identifier of the order whose payment is being rejected.</param>
/// <param name="Notes">Optional notes explaining the rejection reason.</param>
public record AdminRejectPaymentCommand(string OrderId, string? Notes) : ICommand<AdminRejectPaymentResult>;

/// <summary>
/// Result returned after successfully rejecting an order payment.
/// </summary>
/// <param name="IsSuccess">Indicates whether the payment was successfully rejected.</param>
public record AdminRejectPaymentResult(bool IsSuccess);
