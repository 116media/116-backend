using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Contracts.Application;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoById;

/// <summary>
/// Handles the <see cref="AdminGetVideoByIdQuery" /> to retrieve a single video by its identifier.
/// Enriches the response with the author's profile (user name, email, avatar URL, role) by:
/// 1. Resolving the author's identity info via <see cref="IUserLookupService" />
/// 2. Resolving the avatar file URL via <see cref="IFileRepository" /> if the author has an avatar
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="userLookup">Cross-module service for resolving author profiles.</param>
/// <param name="fileRepository">Repository for resolving avatar file URLs.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetVideoByIdHandler(
    IVideoRepository videoRepository,
    IUserLookupService userLookup,
    IFileRepository fileRepository,
    IMapper mapper
) : IQueryHandler<AdminGetVideoByIdQuery, AdminGetVideoByIdResult>
{
    /// <inheritdoc />
    public async Task<AdminGetVideoByIdResult> Handle(AdminGetVideoByIdQuery query, CancellationToken cancellationToken)
    {
        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(
            id: query.Id,
            cancellationToken: cancellationToken
        );

        var dto = await video.ToVideoDetailDtoAsync(mapper, fileRepository, cancellationToken);

        AuthorInfo? authorInfo = await userLookup.GetAuthorInfoByIdAsync(userId: video.AuthorId, ct: cancellationToken);

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

        return new AdminGetVideoByIdResult(Video: dto with { Author = author });
    }
}
