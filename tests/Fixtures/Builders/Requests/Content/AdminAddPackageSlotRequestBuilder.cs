using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot.V1;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminAddPackageSlotRequest"/> instances in tests.
/// Defaults produce values that satisfy <c>AdminAddPackageSlotValidator</c>.
/// </summary>
public class AdminAddPackageSlotRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private Guid? _categoryId;
    private bool _isRequired;
    private int _quantity;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminAddPackageSlotRequestBuilder"/> class
    /// with valid values that satisfy the validator. The category defaults to null (open slot).
    /// </summary>
    public AdminAddPackageSlotRequestBuilder()
    {
        _categoryId = null;
        _isRequired = true;
        _quantity = _faker.Random.Int(min: 1, max: 5);
    }

    /// <summary>
    /// Sets the optional category identifier for the slot. Null creates an open slot.
    /// </summary>
    /// <param name="categoryId">The category identifier, or null for an open slot.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminAddPackageSlotRequestBuilder WithCategoryId(Guid? categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    /// <summary>
    /// Sets whether the slot must be fulfilled for the package to be complete.
    /// </summary>
    /// <param name="isRequired">True if the slot is required.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminAddPackageSlotRequestBuilder WithIsRequired(bool isRequired)
    {
        _isRequired = isRequired;
        return this;
    }

    /// <summary>
    /// Sets the number of content pieces required for this slot.
    /// </summary>
    /// <param name="quantity">The slot quantity.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminAddPackageSlotRequestBuilder WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminAddPackageSlotRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminAddPackageSlotRequest instance.</returns>
    public AdminAddPackageSlotRequest Build()
    {
        return new AdminAddPackageSlotRequest(CategoryId: _categoryId, IsRequired: _isRequired, Quantity: _quantity);
    }
}
