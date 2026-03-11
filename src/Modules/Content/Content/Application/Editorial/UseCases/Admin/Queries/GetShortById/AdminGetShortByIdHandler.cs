using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetShortById;

/// <summary>
/// Handles the <see cref="AdminGetShortByIdQuery" /> to retrieve a single short video by its identifier.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetShortByIdHandler(IShortVideoRepository shortVideoRepository, IMapper mapper)
    : IQueryHandler<AdminGetShortByIdQuery, AdminGetShortByIdResult>
{
    /// <inheritdoc />
    public async Task<AdminGetShortByIdResult> Handle(AdminGetShortByIdQuery query, CancellationToken cancellationToken)
    {
        Guid id = Guid.Parse(query.Id);

        ShortVideoEntity? shortVideo = await shortVideoRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        var dto = shortVideo.ToShortVideoDto(mapper);
        return new AdminGetShortByIdResult(ShortVideo: dto);
    }
}
