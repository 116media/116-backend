using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.Services;
using _116.Identity.Contracts.Application.Services;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetShortById;

/// <summary>
/// Handles the <see cref="AdminGetShortByIdQuery" /> to retrieve a single short video by its identifier.
/// Enriches the response with the author's profile via <see cref="IUserLookupService" />.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="userLookup">Cross-module service for resolving author profiles.</param>
/// <param name="fileStorage">Core's storage contract.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetShortByIdHandler(
    IShortVideoRepository shortVideoRepository,
    IUserLookupService userLookup,
    IFileStorageService fileStorage,
    IMapper mapper,
    IVideoRepository videoRepository
) : IQueryHandler<AdminGetShortByIdQuery, AdminGetShortByIdResult>
{
    /// <inheritdoc />
    public async Task<AdminGetShortByIdResult> Handle(AdminGetShortByIdQuery query, CancellationToken cancellationToken)
    {
        ShortVideoEntity shortVideo = await shortVideoRepository.GetByIdOrThrowAsync(
            id: query.Id,
            cancellationToken: cancellationToken
        );

        ShortVideoDto dto = await shortVideo.ToShortVideoDtoAsync(
            mapper,
            userLookup,
            fileStorage,
            videoRepository,
            cancellationToken
        );

        return new AdminGetShortByIdResult(ShortVideo: dto);
    }
}
