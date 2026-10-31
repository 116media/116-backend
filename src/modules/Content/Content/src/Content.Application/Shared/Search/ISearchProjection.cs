using _116.Content.Domain.Enums;

namespace _116.Content.Application.Shared.Search;

/// <summary>
/// The write side of search, as the domain event handlers see it. Routes to the projector
/// registered for the type, so a handler never names one.
/// </summary>
public interface ISearchProjection
{
    /// <summary>
    /// Reprojects one item from its source. A type with no registered projector is ignored,
    /// so an event for a type that is not searchable costs nothing.
    /// </summary>
    /// <param name="type">The content type the item belongs to.</param>
    /// <param name="entityId">The source aggregate's identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task ProjectAsync(EnumCoreContentType type, Guid entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes one item's document, for a source that no longer exists.
    /// </summary>
    /// <param name="type">The content type the item belonged to.</param>
    /// <param name="entityId">The source aggregate's identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task RemoveAsync(EnumCoreContentType type, Guid entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reprojects every searchable type, for recovery when the projection has drifted.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The number of documents written per type.</returns>
    Task<IReadOnlyDictionary<EnumCoreContentType, int>> RebuildAllAsync(CancellationToken cancellationToken = default);
}
