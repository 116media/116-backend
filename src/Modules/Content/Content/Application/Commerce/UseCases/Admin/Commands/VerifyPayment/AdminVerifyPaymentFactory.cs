using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment.Contracts;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;

/// <summary>
/// Factory implementation for the payment verification flow. Verifies the
/// payment and marks the order paid in one transaction; the order raises
/// <c>OrderPaidEvent</c> with the paid-for effects computed here at raise
/// time from the payment's verification instant, and the post-commit event
/// handlers stamp the content and send the receipt.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminVerifyPaymentFactory(
    ILookupRepository lookupRepository,
    IContentOrderRepository contentOrderRepository,
    IContentUnitOfWork unitOfWork
) : IVerifyPaymentFactory
{
    /// <inheritdoc />
    public async Task VerifyAsync(
        ContentOrderEntity order,
        ContentPaymentEntity payment,
        Guid adminUserId,
        string receiptUrl,
        CancellationToken cancellationToken
    )
    {
        payment.Verify(adminUserId: adminUserId, receiptUrl: receiptUrl);

        IReadOnlyDictionary<Guid, int> promotionDurations = await ResolvePromotionDurationsAsync(
            order: order,
            cancellationToken: cancellationToken
        );

        order.MarkPaid(
            paymentId: payment.Id,
            verifiedAt: payment.VerifiedAt!.Value,
            promotionDurationsByLevelId: promotionDurations
        );

        await contentOrderRepository.UpdatePaymentAsync(payment: payment, ct: cancellationToken);
        await contentOrderRepository.UpdateAsync(order: order, ct: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Resolves the promotion duration in days for every promotion level
    /// referenced by the order's items, so the order can compute each item's
    /// promotion window when it raises <c>OrderPaidEvent</c>.
    /// </summary>
    /// <param name="order">The order whose items reference promotion levels.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>Promotion duration in days per promotion level id.</returns>
    private async Task<IReadOnlyDictionary<Guid, int>> ResolvePromotionDurationsAsync(
        ContentOrderEntity order,
        CancellationToken cancellationToken
    )
    {
        Dictionary<Guid, int> durations = [];

        IEnumerable<Guid> promotionLevelIds = order
            .Items.Where(item => item.PromotionLevelId.HasValue)
            .Select(item => item.PromotionLevelId!.Value)
            .Distinct();

        foreach (Guid promotionLevelId in promotionLevelIds)
        {
            PromotionLevelEntity promotionLevel = await lookupRepository.GetPromotionLevelByIdOrThrowAsync(
                id: promotionLevelId,
                cancellationToken: cancellationToken
            );

            durations[promotionLevelId] = promotionLevel.DurationDays;
        }

        return durations;
    }
}
