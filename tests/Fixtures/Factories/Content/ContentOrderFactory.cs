using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="ContentOrderBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class ContentOrderFactory
{
    /// <summary>
    /// Creates an order with default random values (Draft status).
    /// </summary>
    public static ContentOrderEntity Create() => new ContentOrderBuilder().Build();

    /// <summary>
    /// Creates an order with a specific ID.
    /// </summary>
    public static ContentOrderEntity CreateWithId(Guid id) => new ContentOrderBuilder().WithId(id).Build();

    /// <summary>
    /// Creates an order in PendingPayment status.
    /// </summary>
    public static ContentOrderEntity CreateSubmitted() => new ContentOrderBuilder().AsSubmitted().Build();

    /// <summary>
    /// Creates an order in Paid status.
    /// </summary>
    public static ContentOrderEntity CreatePaid() => new ContentOrderBuilder().AsPaid().Build();

    /// <summary>
    /// Creates an order in Cancelled status.
    /// </summary>
    public static ContentOrderEntity CreateCancelled() => new ContentOrderBuilder().AsCancelled().Build();

    /// <summary>
    /// Creates an order with a specific customer ID.
    /// </summary>
    public static ContentOrderEntity CreateForCustomer(Guid customerId) =>
        new ContentOrderBuilder().WithCustomerId(customerId).Build();
}
