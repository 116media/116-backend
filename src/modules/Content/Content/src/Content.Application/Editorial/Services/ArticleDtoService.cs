using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;
using MapsterMapper;

namespace _116.Content.Application.Editorial.Services;

/// <summary>
/// Assembles article response DTOs through the mapper extensions, owning the lookups, the author
/// profile and the file URL resolution.
/// </summary>
/// <param name="mapper">The Mapster mapper.</param>
/// <param name="userLookup">Service resolving author profiles from the Identity module.</param>
/// <param name="fileStorage">Storage contract resolving cover image and avatar URLs.</param>
/// <param name="contentLookupService">Resolver for the categories, customers and tags the DTOs name.</param>
public class ArticleDtoService(
    IMapper mapper,
    IUserLookupService userLookup,
    IFileStorageService fileStorage,
    IContentLookupService contentLookupService
) : IArticleDtoService
{
    /// <inheritdoc />
    public async Task<ArticleDetailDto> CreateDetailAsync(ArticleEntity article, CancellationToken ct = default)
    {
        ContentLookups lookups = await contentLookupService.ResolveForArticlesAsync([article], ct);

        return await article.ToArticleDetailDtoAsync(mapper, lookups, fileStorage, ct);
    }

    /// <inheritdoc />
    public async Task<ArticleDetailDto> CreateDetailWithAuthorAsync(
        ArticleEntity article,
        CancellationToken ct = default
    )
    {
        ArticleDetailDto dto = await CreateDetailAsync(article, ct);
        UserProfileDto? profile = await userLookup.GetUserProfileByIdAsync(userId: article.AuthorId, ct: ct);

        if (profile is null)
        {
            return dto;
        }

        string? avatarUrl = null;

        if (profile.AvatarFileId is { } avatarFileId)
        {
            FileReferenceDto? avatar = await fileStorage.ResolveAsync(avatarFileId, ct);
            avatarUrl = avatar?.StorageUrl;
        }

        return dto with
        {
            Author = new AdminAuthorDto(
                UserName: profile.UserName,
                Email: profile.Email,
                AvatarUrl: avatarUrl,
                Role: profile.Role
            ),
        };
    }

    /// <inheritdoc />
    public async Task<PublicArticleDetailDto> CreatePublicDetailAsync(
        ArticleEntity article,
        bool isLiked = false,
        bool isBookmarked = false,
        CancellationToken ct = default
    )
    {
        ContentLookups lookups = await contentLookupService.ResolveForArticlesAsync([article], ct);

        return await article.ToPublicArticleDetailDtoAsync(
            mapper,
            lookups,
            fileStorage,
            ct,
            isLiked: isLiked,
            isBookmarked: isBookmarked
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PublicArticleSummaryDto>> CreatePublicManyAsync(
        IReadOnlyList<ArticleEntity> articles,
        CancellationToken ct = default
    )
    {
        ContentLookups lookups = await contentLookupService.ResolveForArticlesAsync(articles, ct);

        return await articles.ToPublicArticleSummaryDtosAsync(lookups, fileStorage, ct);
    }

    /// <inheritdoc />
    public Task<ContentLookups> ResolveLookupsAsync(
        IReadOnlyList<ArticleEntity> articles,
        CancellationToken ct = default
    )
    {
        return contentLookupService.ResolveForArticlesAsync(articles, ct);
    }

    /// <inheritdoc />
    public Task<PublicArticleSummaryDto> CreatePublicSummaryAsync(
        ArticleEntity article,
        ContentLookups lookups,
        IReadOnlySet<Guid> likedArticleIds,
        IReadOnlySet<Guid> bookmarkedArticleIds,
        CancellationToken ct = default
    )
    {
        return article.ToPublicArticleSummaryDtoAsync(lookups, fileStorage, likedArticleIds, bookmarkedArticleIds, ct);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<PublicArticleSummaryDto>> CreatePublicManyAsync(
        IReadOnlyList<ArticleEntity> articles,
        ContentLookups lookups,
        IReadOnlySet<Guid> likedArticleIds,
        IReadOnlySet<Guid> bookmarkedArticleIds,
        CancellationToken ct = default
    )
    {
        return articles.ToPublicArticleSummaryDtosAsync(
            lookups,
            fileStorage,
            likedArticleIds,
            bookmarkedArticleIds,
            ct
        );
    }
}
