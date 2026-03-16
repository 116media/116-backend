using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShortBySlug;

/// <summary>
/// Handles the <see cref="PublicGetPublicShortBySlugQuery" /> to retrieve a single active short video by its slug.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetPublicShortBySlugHandler(IShortVideoRepository shortVideoRepository, IMapper mapper)
    : IQueryHandler<PublicGetPublicShortBySlugQuery, PublicGetPublicShortBySlugResult>
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
            throw ShortVideoErrors.NotFound(Guid.Empty);
        }

        var dto = shortVideo.ToShortVideoDto(mapper);
        return new PublicGetPublicShortBySlugResult(ShortVideo: dto);
    }
}
