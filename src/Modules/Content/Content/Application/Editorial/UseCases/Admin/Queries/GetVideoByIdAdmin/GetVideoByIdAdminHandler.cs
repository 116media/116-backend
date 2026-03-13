using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoByIdAdmin;

/// <summary>
/// Handles the <see cref="GetVideoByIdAdminQuery" /> to retrieve a single video by its identifier.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class GetVideoByIdAdminHandler(IVideoRepository videoRepository, IMapper mapper)
    : IQueryHandler<GetVideoByIdAdminQuery, GetVideoByIdAdminResult>
{
    /// <inheritdoc />
    public async Task<GetVideoByIdAdminResult> Handle(GetVideoByIdAdminQuery query, CancellationToken cancellationToken)
    {
        Guid id = Guid.Parse(query.Id);

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        var dto = video.ToVideoDetailDto(mapper);
        return new GetVideoByIdAdminResult(Video: dto);
    }
}
