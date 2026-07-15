using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Identity.Contracts.Application;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShortBySlug;

/// <summary>
/// Handles the <see cref="PublicGetPublicShortBySlugQuery" /> to retrieve a single active short video by its slug.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="userLookup">Service for resolving the author profile.</param>
/// <param name="fileRepository">Repository for resolving video and thumbnail file URLs.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicGetPublicShortBySlugHandler(
    IShortVideoRepository shortVideoRepository,
    IUserLookupService userLookup,
    IFileRepository fileRepository,
    IMapper mapper,
    ContentI18n i18n
) : IQueryHandler<PublicGetPublicShortBySlugQuery, PublicGetPublicShortBySlugResult>
{
    /// <inheritdoc />
    public async Task<PublicGetPublicShortBySlugResult> Handle(
        PublicGetPublicShortBySlugQuery query,
        CancellationToken cancellationToken
    )
    {
        ShortVideoEntity? shortVideo = await shortVideoRepository.GetBySlugAsync(
            slug: query.Slug,
            cancellationToken: cancellationToken
        );

        if (shortVideo is null || !shortVideo.IsActive)
        {
            throw i18n.ShortVideo.NotFound(Guid.Empty);
        }

        bool isLiked = false;
        bool isBookmarked = false;

        if (query.CurrentUserId is Guid userId)
        {
            isLiked = await shortVideoRepository.HasLikedAsync(userId, shortVideo.Id, cancellationToken);
            isBookmarked = await shortVideoRepository.HasBookmarkedAsync(userId, shortVideo.Id, cancellationToken);
        }

        ShortVideoDto dto = await shortVideo.ToShortVideoDtoAsync(
            mapper,
            userLookup,
            fileRepository,
            cancellationToken,
            isLiked: isLiked,
            isBookmarked: isBookmarked
        );
        return new PublicGetPublicShortBySlugResult(ShortVideo: dto);
    }
}
