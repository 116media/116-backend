using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.Specifications;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShortBySlug;

/// <summary>
/// Handles the <see cref="PublicGetPublicShortBySlugQuery" /> to serve an active short.
/// </summary>
/// <param name="shortVideoRepository">Repository resolving the short and the viewer's interaction state.</param>
/// <param name="shortVideoDtoService">Service assembling the public short DTO.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicGetPublicShortBySlugHandler(
    IShortVideoRepository shortVideoRepository,
    IShortVideoDtoService shortVideoDtoService,
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

        if (shortVideo is null || !new ActiveShortVideoSpecification().IsSatisfiedBy(shortVideo))
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

        PublicShortVideoDto dto = await shortVideoDtoService.CreatePublicAsync(
            shortVideo,
            isLiked,
            isBookmarked,
            cancellationToken
        );
        return new PublicGetPublicShortBySlugResult(ShortVideo: dto);
    }
}
