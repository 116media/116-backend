using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="ContentPaymentBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class ContentPaymentFactory
{
    /// <summary>
    /// Creates a payment for the given order with specified amount (Pending status).
    /// </summary>
    public static ContentPaymentEntity Create(
        Guid orderId,
        decimal amountUsd = TestConstants.Commerce.ValidTotalAmountUsd
    ) => new ContentPaymentBuilder().WithOrderId(orderId).WithAmountUsd(amountUsd).Build();

    /// <summary>
    /// Creates a payment with an attached proof file.
    /// </summary>
    public static ContentPaymentEntity CreateWithProof(Guid orderId, Guid proofFileId) =>
        new ContentPaymentBuilder()
            .WithOrderId(orderId)
            .WithProofFileId(proofFileId, EnumPaymentMethod.BankTransfer)
            .Build();

    /// <summary>
    /// Creates a verified payment.
    /// </summary>
    public static ContentPaymentEntity CreateVerified(Guid orderId) =>
        new ContentPaymentBuilder()
            .WithOrderId(orderId)
            .AsVerified(Guid.NewGuid(), TestConstants.Commerce.ValidReceiptUrl)
            .Build();

    /// <summary>
    /// Creates a rejected payment.
    /// </summary>
    public static ContentPaymentEntity CreateRejected(Guid orderId) =>
        new ContentPaymentBuilder().WithOrderId(orderId).AsRejected(TestConstants.Commerce.ValidRejectionNotes).Build();

    /// <summary>
    /// Creates a payment with default random values.
    /// </summary>
    public static ContentPaymentEntity CreateDefault() => Create(Guid.NewGuid());
}
