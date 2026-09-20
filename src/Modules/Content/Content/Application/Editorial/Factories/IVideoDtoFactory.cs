using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;

namespace _116.Content.Application.Editorial.Factories;

/// <summary>
/// Builds video projections, resolving every thumbnail in a single batch so callers never issue one
/// file query per video.
/// </summary>
public interface IVideoDtoFactory
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
}
