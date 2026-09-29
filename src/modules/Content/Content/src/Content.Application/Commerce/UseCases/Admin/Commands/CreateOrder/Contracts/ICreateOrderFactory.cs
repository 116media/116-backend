using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder.Contracts;

/// <summary>
/// Factory for populating a draft order with items and tiers from a package's slots.
/// </summary>
public interface ICreateOrderFactory
{
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
