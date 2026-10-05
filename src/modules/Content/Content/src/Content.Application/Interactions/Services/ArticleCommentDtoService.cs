using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;

namespace _116.Content.Application.Interactions.Services;

/// <summary>
/// Assembles article comment DTOs, resolving the author profile and avatar URL.
/// </summary>
/// <param name="userLookup">Service resolving author profiles from the Identity module.</param>
/// <param name="fileStorage">Storage contract resolving avatar URLs.</param>
public class ArticleCommentDtoService(IUserLookupService userLookup, IFileStorageService fileStorage)
    : IArticleCommentDtoService
{
    /// <inheritdoc />
    public async Task<PublicArticleCommentDto> CreatePublicAsync(
        ArticleCommentEntity comment,
        CancellationToken ct = default
    )
    {
        UserProfileDto? profile = await userLookup.GetUserProfileByIdAsync(userId: comment.UserId, ct: ct);

        if (profile is null)
        {
            return comment.ToPublicArticleCommentDto();
        }

        string? avatarUrl = null;

        if (profile.AvatarFileId is { } avatarFileId)
        {
            FileReferenceDto? avatar = await fileStorage.ResolveAsync(avatarFileId, ct);
            avatarUrl = avatar?.StorageUrl;
        }

        return comment.ToPublicArticleCommentDto() with
        {
            Author = new PublicAuthorDto(UserName: profile.UserName, AvatarUrl: avatarUrl),
        };
    }
}
