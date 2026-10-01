using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Storage.Contracts.Application.DTOs;

namespace _116.Content.Application.Editorial.Services;

/// <summary>
/// Builds video projections, resolving every thumbnail in a single batch so callers never issue one
/// file query per video.
/// </summary>
public interface IVideoDtoService
{
    /// <summary>
    /// Builds the admin detail projection for one video.
    /// </summary>
    /// <param name="video">The video to project.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The detail projection.</returns>
    Task<VideoDetailDto> CreateDetailAsync(VideoEntity video, CancellationToken ct = default);

    /// <summary>
    /// Builds the public detail projection for one video, stamping the viewer's own rating.
    /// </summary>
    /// <param name="video">The video to project.</param>
    /// <param name="ratedStars">The viewer's rating, when they have rated it.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The detail projection.</returns>
    Task<PublicVideoDetailDto> CreatePublicDetailAsync(
        VideoEntity video,
        short? ratedStars = null,
        CancellationToken ct = default
    );

    /// <summary>
    /// Builds the admin summary projections for a list of videos, resolving every thumbnail in one
    /// query.
    /// </summary>
    /// <param name="videos">The videos to project.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The projections, in the order supplied.</returns>
    Task<IReadOnlyList<VideoSummaryDto>> CreateManyAsync(
        IReadOnlyList<VideoEntity> videos,
        CancellationToken ct = default
    );

    /// <summary>
    /// Builds the public card projections for a list of videos, resolving every thumbnail in one
    /// query.
    /// </summary>
    /// <param name="videos">The videos to project.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The projections, in the order supplied.</returns>
    Task<IReadOnlyList<PublicVideoSummaryDto>> CreatePublicManyAsync(
        IReadOnlyList<VideoEntity> videos,
        CancellationToken ct = default
    );

    /// <summary>
    /// Resolves the thumbnails a set of videos reference, for a caller assembling several
    /// projections from one batch.
    /// </summary>
    /// <param name="videos">The videos whose thumbnails to resolve.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The thumbnails, keyed by file id.</returns>
    Task<IReadOnlyDictionary<Guid, FileReferenceDto>> ResolveThumbnailsAsync(
        IReadOnlyList<VideoEntity> videos,
        CancellationToken ct = default
    );

    /// <summary>
    /// Resolves which of the given videos have a published lyrics page linked, in one query,
    /// for a caller assembling several projections from one batch.
    /// </summary>
    /// <param name="videos">The videos to probe.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The ids of the videos with published lyrics.</returns>
    Task<IReadOnlySet<Guid>> ResolveVideosWithLyricsAsync(
        IReadOnlyList<VideoEntity> videos,
        CancellationToken ct = default
    );

    /// <summary>
    /// Resolves in one batch everything the public summaries of a set of videos need, for callers
    /// assembling several groups from the same batch.
    /// </summary>
    /// <param name="videos">The videos.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<PublicVideoSummaryContext> ResolvePublicContextAsync(
        IReadOnlyList<VideoEntity> videos,
        CancellationToken ct = default
    );

    /// <summary>
    /// Builds public summaries from an already resolved context, without further IO.
    /// </summary>
    /// <param name="videos">The videos.</param>
    /// <param name="context">The context resolved for the batch.</param>
    IReadOnlyList<PublicVideoSummaryDto> CreatePublicMany(
        IReadOnlyList<VideoEntity> videos,
        PublicVideoSummaryContext context
    );
}

/// <summary>
/// Everything the public summaries of a batch of videos read: the lookups, the thumbnails and
/// the published-lyrics fact.
/// </summary>
/// <param name="Lookups">The categories, customers and promotion levels the cards name.</param>
/// <param name="Thumbnails">The resolved thumbnail files by id.</param>
/// <param name="VideosWithLyrics">The ids of the videos with published lyrics.</param>
public record PublicVideoSummaryContext(
    ContentLookups Lookups,
    IReadOnlyDictionary<Guid, FileReferenceDto> Thumbnails,
    IReadOnlySet<Guid> VideosWithLyrics
);
