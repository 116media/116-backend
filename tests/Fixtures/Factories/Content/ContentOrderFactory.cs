using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="ContentOrderEntity"/> instances in tests.
/// </summary>
public static class ContentOrderFactory
{
    /// <summary>Creates an order with default random values (Draft status).</summary>
    public static ContentOrderEntity Create() => new ContentOrderBuilder().Build();

    /// <summary>Creates an order with a specific ID.</summary>
    public static ContentOrderEntity CreateWithId(Guid id) => new ContentOrderBuilder().WithId(id).Build();

    /// <summary>Creates an order in PendingPayment status.</summary>
    public static ContentOrderEntity CreateSubmitted() => new ContentOrderBuilder().AsSubmitted().Build();

    /// <summary>Creates an order in Paid status.</summary>
    public static ContentOrderEntity CreatePaid() => new ContentOrderBuilder().AsPaid().Build();

    /// <summary>Creates an order in Cancelled status.</summary>
    public static ContentOrderEntity CreateCancelled() => new ContentOrderBuilder().AsCancelled().Build();

    /// <summary>Creates an order with a specific customer ID.</summary>
    public static ContentOrderEntity CreateForCustomer(Guid customerId) =>
        new ContentOrderBuilder().WithCustomerId(customerId).Build();

    /// <summary>Creates a list of orders with the specified count.</summary>
    public static List<ContentOrderEntity> CreateMany(int count) =>
        Enumerable.Range(0, count).Select(_ => Create()).ToList();
}
