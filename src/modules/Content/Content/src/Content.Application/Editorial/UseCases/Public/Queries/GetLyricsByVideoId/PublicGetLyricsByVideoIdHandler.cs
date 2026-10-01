using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.Specifications;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId;

/// <summary>
/// Handles the <see cref="PublicGetLyricsByVideoIdQuery" /> to serve the published lyrics of a video.
/// </summary>
/// <param name="lyricsRepository">Repository resolving the lyrics and the like state.</param>
/// <param name="lyricsDtoService">Service assembling the public lyrics detail.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicGetLyricsByVideoIdHandler(
    ILyricsRepository lyricsRepository,
    ILyricsDtoService lyricsDtoService,
    ContentI18n i18n
) : IQueryHandler<PublicGetLyricsByVideoIdQuery, PublicGetLyricsByVideoIdResult>
{
    /// <inheritdoc />
    public async Task<PublicGetLyricsByVideoIdResult> Handle(
        PublicGetLyricsByVideoIdQuery query,
        CancellationToken cancellationToken
    )
    {
        Guid videoId = Guid.Parse(query.VideoId);
        LyricsEntity? lyrics = await lyricsRepository.GetByVideoIdAsync(
            videoId: videoId,
            cancellationToken: cancellationToken
        );

        var published = new LyricsByStatusSpecification(status: EnumContentStatus.Published);

        if (lyrics is null || !published.IsSatisfiedBy(lyrics))
        {
            throw i18n.Lyrics.NotFound(id: videoId);
        }

        bool isLiked =
            query.CurrentUserId is Guid currentUserId
            && await lyricsRepository.HasLikedAsync(
                userId: currentUserId,
                lyricsId: lyrics.Id,
                cancellationToken: cancellationToken
            );

        var dto = await lyricsDtoService.CreatePublicDetailAsync(lyrics, isLiked, cancellationToken);
        return new PublicGetLyricsByVideoIdResult(Lyrics: dto);
    }
}
