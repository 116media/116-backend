using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Persistence.Projections;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Search.Projectors;

/// <summary>
/// Projects short videos into the search documents. Shorts carry no editorial workflow, so
/// their title stays mutable for life and every change reprojects through
/// <c>ShortVideoChangedEvent</c>.
/// </summary>
/// <param name="context">The Content database context.</param>
/// <param name="writer">The shared document writer.</param>
public class ShortVideoSearchProjector(ContentDbContext context, SearchDocumentWriter writer) : ISearchDocumentProjector
{
    /// <inheritdoc />
    public EnumCoreContentType Type => EnumCoreContentType.Short;

    /// <inheritdoc />
    public async Task ProjectAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        ShortVideoEntity? shortVideo = await context
            .ShortVideos.AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == entityId, cancellationToken);

        if (shortVideo is null)
        {
            await writer.RemoveAsync(Type, entityId, cancellationToken);
            return;
        }

        await writer.UpsertAsync(ToDocument(shortVideo), cancellationToken);
    }

    /// <inheritdoc />
    public Task RemoveAsync(Guid entityId, CancellationToken cancellationToken = default) =>
        writer.RemoveAsync(Type, entityId, cancellationToken);

    /// <inheritdoc />
    public async Task<int> RebuildAsync(CancellationToken cancellationToken = default)
    {
        List<ShortVideoEntity> shorts = await context.ShortVideos.AsNoTracking().ToListAsync(cancellationToken);

        return await writer.ReplaceAllAsync([.. shorts.Select(ToDocument)], cancellationToken);
    }

    // A short carries only a title: no teaser, no category and no rating.
    private static SearchDocument ToDocument(ShortVideoEntity shortVideo) =>
        new()
        {
            Type = EnumCoreContentType.Short,
            EntityId = shortVideo.Id,
            Slug = shortVideo.Slug.Value,
            IsVisible = shortVideo.IsActive,
            Title = shortVideo.Title,
            Summary = string.Empty,
            Keywords = string.Empty,
            Body = string.Empty,
            ImageFileId = shortVideo.ThumbnailFileId,
            CategoryId = null,
            PublishedAt = shortVideo.CreatedAt,
        };
}
