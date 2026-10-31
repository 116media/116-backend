using _116.Content.Domain.Enums;

namespace _116.Content.Application.Shared.Search;

/// <summary>
/// Projects one content type into the search documents. Registered rather than enumerated,
/// so a new searchable type is a new implementation and never an edit to the reader, the
/// catalog or the handler that calls this.
/// </summary>
public interface ISearchDocumentProjector
{
    /// <summary>
    /// The content type this projector owns. Must be unique across the registered projectors.
    /// </summary>
    EnumCoreContentType Type { get; }

    /// <summary>
    /// Writes the current state of one item into the search documents, inserting, updating
    /// or hiding it as its source now stands. Idempotent, so replaying an event is safe.
    /// </summary>
    /// <param name="entityId">The source aggregate's identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task ProjectAsync(Guid entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes one item's document, for a source that no longer exists.
    /// </summary>
    /// <param name="entityId">The source aggregate's identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task RemoveAsync(Guid entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reprojects every item of this type, for the first build and for recovery when the
    /// projection has drifted from its sources.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The number of documents written.</returns>
    Task<int> RebuildAsync(CancellationToken cancellationToken = default);
}
