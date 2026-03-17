using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;

/// <summary>
/// Command for verifying an order payment and stamping social boost / featured promotion on linked content.
/// </summary>
/// <param name="OrderId">The identifier of the order whose payment is being verified.</param>
/// <param name="ReceiptUrl">The URL of the official payment receipt.</param>
/// <param name="AdminUserId">The identifier of the admin performing the verification.</param>
public record AdminVerifyPaymentCommand(string OrderId, string ReceiptUrl, Guid AdminUserId)
    : ICommand<AdminVerifyPaymentResult>;

/// <summary>
/// Result returned after successfully verifying an order payment.
/// </summary>
/// <param name="IsSuccess">Indicates whether the payment was successfully verified.</param>
public record AdminVerifyPaymentResult(bool IsSuccess);
