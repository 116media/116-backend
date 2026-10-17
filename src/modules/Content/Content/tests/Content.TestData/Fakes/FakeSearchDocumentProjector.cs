using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Enums;

namespace _116.Content.TestData.Fakes;

/// <summary>
/// A projector that records what it was asked to do instead of writing, so a test can assert
/// routing without a database.
/// </summary>
/// <param name="type">The content type this projector claims.</param>
public class FakeSearchDocumentProjector(EnumCoreContentType type) : ISearchDocumentProjector
{
    /// <summary>
    /// The entity identifiers this projector was asked to project, in call order.
    /// </summary>
    public List<Guid> Projected { get; } = [];

    /// <summary>
    /// The entity identifiers this projector was asked to remove, in call order.
    /// </summary>
    public List<Guid> Removed { get; } = [];

    /// <summary>
    /// How many times this projector was asked to rebuild.
    /// </summary>
    public int Rebuilds { get; private set; }

    /// <inheritdoc />
    public EnumCoreContentType Type => type;

    /// <inheritdoc />
    public Task ProjectAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        Projected.Add(entityId);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        Removed.Add(entityId);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int> RebuildAsync(CancellationToken cancellationToken = default)
    {
        Rebuilds++;

        return Task.FromResult(Projected.Count);
    }
}
