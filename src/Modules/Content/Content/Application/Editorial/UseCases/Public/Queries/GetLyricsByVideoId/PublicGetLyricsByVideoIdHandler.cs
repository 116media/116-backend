using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Identity.Contracts.Application;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId;

/// <summary>
/// Handles the <see cref="PublicGetLyricsByVideoIdQuery" /> to retrieve
/// the lyrics page linked to a given video.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="mapper">The Mapster mapper used for tags.</param>
/// <param name="userLookup">Service for resolving author profiles from the Identity module.</param>
/// <param name="fileRepository">Repository for resolving avatar file URLs.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicGetLyricsByVideoIdHandler(
    ILyricsRepository lyricsRepository,
    IMapper mapper,
    IUserLookupService userLookup,
    IFileRepository fileRepository,
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

        if (lyrics is not null && lyrics.Status == EnumContentStatus.Published)
        {
            bool isLiked =
                query.CurrentUserId is Guid currentUserId
                && await lyricsRepository.HasLikedAsync(
                    userId: currentUserId,
                    lyricsId: lyrics.Id,
                    cancellationToken: cancellationToken
                );

            var dto = await lyrics.ToLyricsDetailDtoAsync(
                mapper,
                userLookup,
                fileRepository,
                cancellationToken,
                isLiked
            );
            return new PublicGetLyricsByVideoIdResult(Lyrics: dto);
        }

        throw i18n.Lyrics.NotFound(id: videoId);
    }
}
