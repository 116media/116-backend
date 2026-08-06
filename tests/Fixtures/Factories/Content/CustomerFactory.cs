using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="CustomerBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class CustomerFactory
{
    /// <summary>
    /// Creates a customer with default random values.
    /// </summary>
    public static CustomerEntity Create() => new CustomerBuilder().Build();

    /// <summary>
    /// Creates a customer with a specific email.
    /// </summary>
    public static CustomerEntity Create(string email) => new CustomerBuilder().WithEmail(email).Build();

    /// <summary>
    /// Creates a customer with a specific ID.
    /// </summary>
    public static CustomerEntity CreateWithId(Guid id) => new CustomerBuilder().WithId(id).Build();

    /// <summary>
    /// Creates a customer with known default values.
    /// </summary>
    public static CustomerEntity CreateDefault() =>
        new CustomerBuilder()
            .WithFullName(TestConstants.Customer.ValidFullName)
            .WithEmail(TestConstants.Customer.ValidEmail)
            .Build();

    /// <summary>
    /// Creates a list of customers with the specified count.
    /// </summary>
    public static List<CustomerEntity> CreateMany(int count) =>
        Enumerable.Range(0, count).Select(_ => Create()).ToList();
}
