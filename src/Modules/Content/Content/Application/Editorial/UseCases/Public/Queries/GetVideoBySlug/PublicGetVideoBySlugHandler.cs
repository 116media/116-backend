using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoBySlug;

/// <summary>
/// Handles the <see cref="PublicGetVideoBySlugQuery" /> to retrieve a single published video by its slug.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetVideoBySlugHandler(IVideoRepository videoRepository, IMapper mapper)
    : IQueryHandler<PublicGetVideoBySlugQuery, PublicGetVideoBySlugResult>
{
    /// <inheritdoc />
    public async Task<PublicGetVideoBySlugResult> Handle(
        PublicGetVideoBySlugQuery query,
        CancellationToken cancellationToken
    )
    {
        VideoEntity? video = await videoRepository.GetBySlugAsync(
            slug: query.Slug,
            cancellationToken: cancellationToken
        );

        if (video is null || video.Status != EnumContentStatus.Published)
        {
            throw VideoErrors.NotFound(Guid.Empty);
        }

        var dto = video.ToVideoDetailDto(mapper);
        return new PublicGetVideoBySlugResult(Video: dto);
    }
}
