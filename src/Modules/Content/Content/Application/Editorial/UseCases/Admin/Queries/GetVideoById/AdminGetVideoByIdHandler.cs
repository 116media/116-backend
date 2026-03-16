using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoById;

/// <summary>
/// Handles the <see cref="AdminGetVideoByIdQuery" /> to retrieve a single video by its identifier.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetVideoByIdHandler(IVideoRepository videoRepository, IMapper mapper)
    : IQueryHandler<AdminGetVideoByIdQuery, AdminGetVideoByIdResult>
{
    /// <inheritdoc />
    public async Task<AdminGetVideoByIdResult> Handle(AdminGetVideoByIdQuery query, CancellationToken cancellationToken)
    {
        Guid id = Guid.Parse(query.Id);

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        var dto = video.ToVideoDetailDto(mapper);
        return new AdminGetVideoByIdResult(Video: dto);
    }
}
