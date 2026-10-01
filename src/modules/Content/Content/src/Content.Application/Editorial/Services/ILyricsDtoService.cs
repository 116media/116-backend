using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.Services;

/// <summary>
/// Assembles lyrics response DTOs, resolving the lookups, author profile and file URLs the wire
/// shape carries.
/// </summary>
public interface ILyricsDtoService
{
    /// <summary>
    /// Builds the admin detail of one lyrics page.
    /// </summary>
    /// <param name="lyrics">The lyrics.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<LyricsDetailDto> CreateDetailAsync(LyricsEntity lyrics, CancellationToken ct = default);

    /// <summary>
    /// Builds the public detail of one lyrics page.
    /// </summary>
    /// <param name="lyrics">The lyrics.</param>
    /// <param name="isLiked">Whether the current user has liked it.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<PublicLyricsDetailDto> CreatePublicDetailAsync(
        LyricsEntity lyrics,
        bool isLiked = false,
        CancellationToken ct = default
    );

    /// <summary>
    /// Builds the public summaries of a list of lyrics.
    /// </summary>
    /// <param name="lyrics">The lyrics.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<IReadOnlyList<PublicLyricsSummaryDto>> CreatePublicManyAsync(
        IReadOnlyList<LyricsEntity> lyrics,
        CancellationToken ct = default
    );
}
