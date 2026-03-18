using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderPayment;

/// <summary>
/// Query for retrieving the payment record of a specific order.
/// </summary>
/// <param name="OrderId">The identifier of the order.</param>
public record AdminGetOrderPaymentQuery(string OrderId) : IQuery<AdminGetOrderPaymentResult>;

/// <summary>
/// Result of the <see cref="AdminGetOrderPaymentQuery" /> containing the payment details.
/// </summary>
/// <param name="Payment">The payment information.</param>
public record AdminGetOrderPaymentResult(PaymentDto Payment);
