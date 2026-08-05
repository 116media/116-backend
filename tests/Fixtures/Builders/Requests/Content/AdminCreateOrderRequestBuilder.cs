using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder.V1;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminCreateOrderRequest"/> instances in tests.
/// </summary>
public class AdminCreateOrderRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _customerId;
    private Guid? _packageId;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminCreateOrderRequestBuilder"/> class
    /// with valid random values that satisfy the validator.
    /// </summary>
    public AdminCreateOrderRequestBuilder()
    {
        _customerId = _faker.Random.Guid().ToString();
        _packageId = null;
    }

    /// <summary>
    /// Sets the identifier of the B2B customer placing the order.
    /// </summary>
    /// <param name="customerId">The customer identifier as a string GUID.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateOrderRequestBuilder WithCustomerId(string customerId)
    {
        _customerId = customerId;
        return this;
    }

    /// <summary>
    /// Sets the optional pre-configured package to apply to the order.
    /// </summary>
    /// <param name="packageId">The package identifier, or null for no package.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateOrderRequestBuilder WithPackageId(Guid? packageId)
    {
        _packageId = packageId;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminCreateOrderRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminCreateOrderRequest instance.</returns>
    public AdminCreateOrderRequest Build()
    {
        return new AdminCreateOrderRequest(CustomerId: _customerId, PackageId: _packageId);
    }
}
