using _116.Content.Domain.Enums;

namespace _116.Content.Application.Shared.Search;

/// <summary>
/// Routes a projection request to the projector registered for the type. A type with no
/// projector is not searchable yet, which is a no-op rather than an error.
/// </summary>
/// <param name="projectors">Every registered per-type projector.</param>
public class SearchProjection(IEnumerable<ISearchDocumentProjector> projectors) : ISearchProjection
{
    /// <inheritdoc />
    public Task ProjectAsync(EnumCoreContentType type, Guid entityId, CancellationToken cancellationToken = default)
    {
        return Find(type)?.ProjectAsync(entityId, cancellationToken) ?? Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(EnumCoreContentType type, Guid entityId, CancellationToken cancellationToken = default)
    {
        return Find(type)?.RemoveAsync(entityId, cancellationToken) ?? Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<EnumCoreContentType, int>> RebuildAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        var written = new Dictionary<EnumCoreContentType, int>(Projectors.Length);

        foreach (ISearchDocumentProjector projector in Projectors)
        {
            written[projector.Type] = await projector.RebuildAsync(cancellationToken);
        }

        return written;
    }

    private ISearchDocumentProjector? Find(EnumCoreContentType type)
    {
        return Array.Find(Projectors, candidate => candidate.Type == type);
    }

    /// <summary>
    /// The projectors, materialised once so routing does not re-enumerate the container.
    /// </summary>
    private ISearchDocumentProjector[] Projectors { get; } = [.. projectors];
}
