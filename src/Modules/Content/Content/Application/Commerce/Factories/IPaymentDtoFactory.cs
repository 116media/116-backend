using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;

namespace _116.Content.Application.Commerce.Factories;

/// <summary>
/// Builds payment projections, resolving every verifier's name in a single batch so callers never
/// issue one user lookup per payment.
/// </summary>
public interface IPaymentDtoFactory
{
    /// <summary>
    /// Builds the full projection for one payment, including its proof file.
    /// </summary>
    /// <param name="payment">The payment to project.</param>
    /// <param name="proofFile">The resolved payment proof, when the order carries one.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The projection.</returns>
    Task<PaymentDto> CreateAsync(
        ContentPaymentEntity payment,
        FileDto? proofFile = null,
        CancellationToken ct = default
    );

    /// <summary>
    /// Builds the summary projections for the payments carried by a list of orders, resolving
    /// every verifier and customer in one query each.
    /// </summary>
    /// <param name="orders">The orders whose payments to project; each must carry a payment.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The projections, in the order supplied.</returns>
    Task<IReadOnlyList<PaymentSummaryDto>> CreateManyAsync(
        IReadOnlyList<ContentOrderEntity> orders,
        CancellationToken ct = default
    );
}
