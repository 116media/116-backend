using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="CustomerEntity"/> instances in tests.
/// </summary>
public static class CustomerFactory
{
    /// <summary>Creates a customer with default random values.</summary>
    public static CustomerEntity Create() => new CustomerBuilder().Build();

    /// <summary>Creates a customer with a specific email.</summary>
    public static CustomerEntity Create(string email) => new CustomerBuilder().WithEmail(email).Build();

    /// <summary>Creates a customer with a specific ID.</summary>
    public static CustomerEntity CreateWithId(Guid id) => new CustomerBuilder().WithId(id).Build();

    /// <summary>Creates a customer with known default values.</summary>
    public static CustomerEntity CreateDefault() =>
        new CustomerBuilder()
            .WithFullName(TestConstants.Content.Customer.ValidFullName)
            .WithEmail(TestConstants.Content.Customer.ValidEmail)
            .Build();

    /// <summary>Creates a list of customers with the specified count.</summary>
    public static List<CustomerEntity> CreateMany(int count) =>
        Enumerable.Range(0, count).Select(_ => Create()).ToList();
}
