using _116.Content.Application.Editorial.Factories;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoById;

/// <summary>
/// Handles the <see cref="AdminGetVideoByIdQuery" /> to retrieve a single video by its identifier.
/// Enriches the response with the author's profile (user name, email, avatar URL, role) by:
/// 1. Resolving the author's identity info via <see cref="IUserLookupService" />
/// 2. Resolving the avatar file URL via <see cref="IFileRepository" /> if the author has an avatar
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="userLookup">Cross-module service for resolving author profiles.</param>
/// <param name="fileStorage">Core's storage contract.</param>
/// <param name="videoDtoFactory">Builds video projections with their thumbnails resolved.</param>
public class AdminGetVideoByIdHandler(
    IVideoRepository videoRepository,
    IUserLookupService userLookup,
    IVideoDtoFactory videoDtoFactory,
    IFileStorageService fileStorage
) : IQueryHandler<AdminGetVideoByIdQuery, AdminGetVideoByIdResult>
{
    /// <inheritdoc />
    public async Task<AdminGetVideoByIdResult> Handle(AdminGetVideoByIdQuery query, CancellationToken cancellationToken)
    {
        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(
            id: query.Id,
            cancellationToken: cancellationToken
        );

        var dto = await videoDtoFactory.CreateDetailAsync(video, cancellationToken);

        AuthorDto? authorInfo = await userLookup.GetAuthorInfoByIdAsync(userId: video.AuthorId, ct: cancellationToken);

        AdminAuthorDto? author = null;
        if (authorInfo is not null)
        {
            string? avatarUrl = null;
            if (authorInfo.AvatarFileId.HasValue)
            {
                FileReferenceDto? avatarFile = await fileStorage.ResolveAsync(
                    authorInfo.AvatarFileId.Value,
                    cancellationToken
                );
                avatarUrl = avatarFile?.StorageUrl;
            }

            author = new AdminAuthorDto(
                UserName: authorInfo.UserName,
                Email: authorInfo.Email,
                AvatarUrl: avatarUrl,
                Role: authorInfo.Role
            );
        }

        return new AdminGetVideoByIdResult(Video: dto with { Author = author });
    }
}
