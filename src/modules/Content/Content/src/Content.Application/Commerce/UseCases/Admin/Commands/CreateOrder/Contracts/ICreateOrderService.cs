using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder.Contracts;

/// <summary>
/// Service for populating a draft order with items and tiers from a package's slots.
/// </summary>
/// <summary>
/// The order just created, its customer and the number of items the package seeded.
/// </summary>
/// <param name="Order">The staged order.</param>
/// <param name="Customer">The commissioning customer.</param>
/// <param name="ItemCount">The items seeded from the package, zero without one.</param>
public record CreatedOrderData(ContentOrderEntity Order, CustomerEntity Customer, int ItemCount);

public interface ICreateOrderService
{
    /// <summary>
    /// Resolves the customer and the optional package, stages the order and seeds its items from
    /// the package. Throws the localized error when either is missing or the package is inactive.
    /// The caller owns the commit.
    /// </summary>
    /// <param name="customerId">The commissioning customer.</param>
    /// <param name="packageId">The package to seed from, or null for an empty order.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<CreatedOrderData> CreateAsync(Guid customerId, Guid? packageId, CancellationToken ct);

    /// <summary>
    /// Creates order items and their pricing tiers from the package's slots.
    /// Fetches all category pricing in a single batch per category, then creates
    /// items and tiers without nested async loops.
    /// </summary>
    /// <param name="order">The draft order to populate.</param>
    /// <param name="package">The package whose slots define the items to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The number of items created.</returns>
    Task<int> PopulateFromPackageAsync(ContentOrderEntity order, PackageEntity package, CancellationToken ct);
}
