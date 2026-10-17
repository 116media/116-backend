using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Persistence.Projections;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Search.Projectors;

/// <summary>
/// Projects videos into the search documents. A video's searchable text is frozen once it is
/// published, so in practice only publication state and the SEO and thumbnail fields move
/// after the first projection.
/// </summary>
/// <param name="context">The Content database context.</param>
/// <param name="writer">The shared document writer.</param>
public class VideoSearchProjector(ContentDbContext context, SearchDocumentWriter writer) : ISearchDocumentProjector
{
    /// <inheritdoc />
    public EnumCoreContentType Type => EnumCoreContentType.Video;

    /// <inheritdoc />
    public async Task ProjectAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        VideoEntity? video = await context
            .Videos.AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == entityId, cancellationToken);

        if (video is null)
        {
            await writer.RemoveAsync(Type, entityId, cancellationToken);
            return;
        }

        await writer.UpsertAsync(ToDocument(video), cancellationToken);
    }

    /// <inheritdoc />
    public Task RemoveAsync(Guid entityId, CancellationToken cancellationToken = default) =>
        writer.RemoveAsync(Type, entityId, cancellationToken);

    /// <inheritdoc />
    public async Task<int> RebuildAsync(CancellationToken cancellationToken = default)
    {
        List<VideoEntity> videos = await context.Videos.AsNoTracking().ToListAsync(cancellationToken);

        return await writer.ReplaceAllAsync([.. videos.Select(ToDocument)], cancellationToken);
    }

    private static SearchDocument ToDocument(VideoEntity video) =>
        new()
        {
            Type = EnumCoreContentType.Video,
            EntityId = video.Id,
            Slug = video.Slug.Value,
            IsVisible = video.Status == EnumContentStatus.Published,
            Title = video.Title,
            Summary = video.Description,
            Keywords = $"{video.MetaTitle} {video.MetaDescription}".Trim(),
            Body = string.Empty,
            ImageFileId = video.ThumbnailFileId,
            CategoryId = video.CategoryId,
            PublishedAt = video.PublishedAt,
            RatingAverage = video.RatingAverage,
        };
}
