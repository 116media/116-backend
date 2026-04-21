using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Contracts.Application;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetShortById;

/// <summary>
/// Handles the <see cref="AdminGetShortByIdQuery" /> to retrieve a single short video by its identifier.
/// Enriches the response with the author's profile via <see cref="IUserLookupService" />.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="userLookup">Cross-module service for resolving author profiles.</param>
/// <param name="fileRepository">Repository for resolving avatar file URLs.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetShortByIdHandler(
    IShortVideoRepository shortVideoRepository,
    IUserLookupService userLookup,
    IFileRepository fileRepository,
    IMapper mapper
) : IQueryHandler<AdminGetShortByIdQuery, AdminGetShortByIdResult>
{
    /// <inheritdoc />
    public async Task<AdminGetShortByIdResult> Handle(AdminGetShortByIdQuery query, CancellationToken cancellationToken)
    {
        ShortVideoEntity? shortVideo = await shortVideoRepository.GetByIdOrThrowAsync(
            id: query.Id,
            cancellationToken: cancellationToken
        );

        var dto = shortVideo.ToShortVideoDto(mapper);

        AuthorInfo? authorInfo = await userLookup.GetAuthorInfoByIdAsync(
            userId: shortVideo.AuthorId,
            ct: cancellationToken
        );

        AuthorDto? author = null;
        if (authorInfo is not null)
        {
            string? avatarUrl = null;
            if (authorInfo.AvatarFileId.HasValue)
            {
                FileEntity? avatarFile = await fileRepository.GetByIdAsync(
                    authorInfo.AvatarFileId.Value,
                    cancellationToken
                );
                avatarUrl = avatarFile?.StorageUrl;
            }

            author = new AuthorDto(
                UserName: authorInfo.UserName,
                Email: authorInfo.Email,
                AvatarUrl: avatarUrl,
                Role: authorInfo.Role
            );
        }

        return new AdminGetShortByIdResult(ShortVideo: dto with { Author = author });
    }
}
