using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Identity.Contracts.Application;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetShortsFeed;

/// <summary>
/// Handles the <see cref="PublicGetShortsFeedQuery" />: resolves the shuffle seed (from the
/// cursor, or fresh), fetches one keyset page of the seeded ordering, stamps per-user like /
/// bookmark flags, and builds the next-page cursor from the last item.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="userLookup">Service for resolving author profiles.</param>
/// <param name="fileRepository">Repository for resolving video and thumbnail file URLs.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetShortsFeedHandler(
    IShortVideoRepository shortVideoRepository,
    IUserLookupService userLookup,
    IFileRepository fileRepository,
    IMapper mapper
) : IQueryHandler<PublicGetShortsFeedQuery, PublicGetShortsFeedResult>
{
    /// <inheritdoc />
    public async Task<PublicGetShortsFeedResult> Handle(
        PublicGetShortsFeedQuery query,
        CancellationToken cancellationToken
    )
    {
        int seed;
        string? afterSortKey = null;
        Guid? afterId = null;

        if (ShortVideoFeedCursor.TryDecode(query.Cursor, out ShortVideoFeedCursor cursor))
        {
            seed = cursor.Seed;
            afterSortKey = cursor.AfterSortKey;
            afterId = cursor.AfterId;
        }
        else
        {
            seed = Random.Shared.Next(1, int.MaxValue);
        }

        IReadOnlyList<ShortVideoEntity> shortVideos = await shortVideoRepository.GetRandomizedFeedAsync(
            seed: seed,
            afterSortKey: afterSortKey,
            afterId: afterId,
            limit: query.PageSize,
            cancellationToken: cancellationToken
        );

        List<Guid> shortVideoIds = shortVideos.Select(shortVideo => shortVideo.Id).ToList();

        (IReadOnlySet<Guid> liked, IReadOnlySet<Guid> bookmarked) =
            await shortVideoRepository.GetLikedAndBookmarkedIdsAsync(
                shortVideoIds: shortVideoIds,
                currentUserId: query.CurrentUserId,
                cancellationToken: cancellationToken
            );

        IReadOnlyList<ShortVideoDto> items = await shortVideos.ToShortVideoDtosAsync(
            mapper,
            userLookup,
            fileRepository,
            liked,
            bookmarked,
            cancellationToken
        );

        string? nextCursor = BuildNextCursor(shortVideos, seed, query.PageSize);

        return new PublicGetShortsFeedResult(Items: items, NextCursor: nextCursor);
    }

    /// <summary>
    /// Builds the next-page cursor from the last returned short video, or returns null when
    /// the page was not full (the feed is exhausted). The sort key is computed in-process to
    /// match the database ordering, avoiding an extra round-trip per page.
    /// </summary>
    private static string? BuildNextCursor(IReadOnlyList<ShortVideoEntity> shortVideos, int seed, int pageSize)
    {
        if (shortVideos.Count == 0 || shortVideos.Count < pageSize)
        {
            return null;
        }

        ShortVideoEntity last = shortVideos[^1];
        string lastSortKey = ShortVideoFeedCursor.ComputeSortKey(last.Id, seed);

        return new ShortVideoFeedCursor(seed, lastSortKey, last.Id).Encode();
    }
}
