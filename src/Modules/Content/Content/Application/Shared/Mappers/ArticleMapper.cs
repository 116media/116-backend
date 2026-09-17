using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for Article and ArticleImage entity mappings.
/// </summary>
public static class ArticleMapper
{
    /// <summary>
    /// Registers Article and ArticleImage entity mappings into the provided TypeAdapterConfig.
    /// </summary>
    /// <remarks>
    /// <c>ArticleEntity → ArticleSummaryDto</c> and <c>ArticleEntity → ArticleDetailDto</c> are
    /// intentionally NOT registered here. Registering them causes Mapster to auto-flatten the
    /// <c>PromotionLevel</c> navigation property (which shares field names like <c>Id</c>,
    /// <c>CreatedAt</c>, <c>CreatedBy</c> with the destination DTO base) and then NPEs at runtime
    /// when <c>PromotionLevel</c> is null. Those two mappings are handled as plain C# in the
    /// extension methods below.
    /// </remarks>
    /// <param name="config">
    /// The TypeAdapterConfig to register mappings into.
    /// </param>
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ArticleImageEntity, ArticleImageDto>();

        config
            .NewConfig<TagEntity, TagDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Slug, src => src.Slug);
    }

    /// <summary>
    /// Maps an <see cref="ArticleEntity" /> to an <see cref="ArticleSummaryDto" />,
    /// resolving the cover image URL from the associated FileEntity.
    /// </summary>
    public static async Task<ArticleSummaryDto> ToArticleSummaryDtoAsync(
        this ArticleEntity entity,
        IMapper mapper,
        ContentLookups lookups,
        IFileStorageService fileStorage,
        CancellationToken ct = default
    )
    {
        string? coverImageUrl = await ResolveCoverImageUrlAsync(entity, fileStorage, ct);

        return entity.ToArticleSummaryDto(mapper, lookups, coverImageUrl: coverImageUrl);
    }

    /// <summary>
    /// Maps an <see cref="ArticleEntity" /> to an <see cref="ArticleSummaryDto" /> from an
    /// already resolved cover URL. Performs no IO — batch mappings resolve files up front.
    /// </summary>
    public static ArticleSummaryDto ToArticleSummaryDto(
        this ArticleEntity entity,
        IMapper mapper,
        ContentLookups lookups,
        string? coverImageUrl
    )
    {
        return new ArticleSummaryDto(
            entity.Id,
            entity.CategoryId,
            lookups.CategoryName(entity.CategoryId),
            entity.Title,
            entity.Slug,
            entity.Headline,
            coverImageUrl,
            entity.AuthorId.ToString(),
            entity.Status,
            entity.IsPromoted,
            entity.PublishedAt,
            entity.LikeCount,
            entity.CommentCount,
            entity.ShareCount,
            entity.BookmarkCount
        )
        {
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
        };
    }

    /// <summary>
    /// Maps an <see cref="ArticleEntity" /> to an <see cref="ArticleDetailDto" />,
    /// resolving the cover image URL from the associated FileReferenceDto and stamping the
    /// current user's interaction flags.
    /// </summary>
    /// <param name="entity">The article to map.</param>
    /// <param name="mapper">The Mapster mapper used for images and tags.</param>
    /// <param name="fileStorage">Core's storage contract.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <param name="isLiked">
    /// Whether the current user has liked this article. False when anonymous.
    /// </param>
    /// <param name="isBookmarked">
    /// Whether the current user has bookmarked this article. False when anonymous.
    /// </param>
    /// <returns>The mapped detail DTO.</returns>
    public static async Task<ArticleDetailDto> ToArticleDetailDtoAsync(
        this ArticleEntity entity,
        IMapper mapper,
        ContentLookups lookups,
        IFileStorageService fileStorage,
        CancellationToken ct = default,
        bool isLiked = false,
        bool isBookmarked = false
    )
    {
        string? coverImageUrl = await ResolveCoverImageUrlAsync(entity, fileStorage, ct);

        return new ArticleDetailDto(
            entity.Id,
            entity.CategoryId,
            lookups.CategoryName(entity.CategoryId),
            entity.Title,
            entity.Slug,
            entity.Headline,
            entity.Body,
            coverImageUrl,
            entity.AuthorId.ToString(),
            entity.Status,
            entity.RejectionReason,
            entity.SocialBoost,
            entity.IsPromoted,
            entity.PromotedUntil,
            entity.PromotionLevelId,
            lookups.PromotionLevelName(entity.PromotionLevelId),
            entity.PublishedAt,
            entity.MetaTitle,
            entity.MetaDescription,
            mapper.Map<IReadOnlyList<ArticleImageDto>>(entity.Images),
            entity.TagDtos(mapper, lookups),
            Math.Max(
                1,
                (int)Math.Ceiling(entity.Body.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length / 200.0)
            ),
            entity.LikeCount,
            entity.CommentCount,
            entity.ShareCount,
            entity.BookmarkCount,
            entity.CustomerId,
            lookups.CustomerName(entity.CustomerId),
            entity.OrderItemId
        )
        {
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
            IsLiked = isLiked,
            IsBookmarked = isBookmarked,
        };
    }

    /// <summary>
    /// Maps a list of <see cref="ArticleEntity" /> to a list of <see cref="ArticleSummaryDto" />,
    /// resolving cover image URLs from associated FileReferenceDto records.
    /// </summary>
    public static async Task<IReadOnlyList<ArticleSummaryDto>> ToArticleSummaryDtosAsync(
        this IReadOnlyList<ArticleEntity> entities,
        IMapper mapper,
        ContentLookups lookups,
        IFileStorageService fileStorage,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> files = await fileStorage.ResolveManyAsync(
            entities.Where(e => e.CoverImageFileId.HasValue).Select(e => e.CoverImageFileId!.Value).Distinct().ToList(),
            ct
        );

        return entities
            .Select(entity =>
                entity.ToArticleSummaryDto(
                    mapper,
                    lookups,
                    coverImageUrl: entity.CoverImageFileId.HasValue
                        ? files.GetValueOrDefault(entity.CoverImageFileId.Value)?.StorageUrl
                        : null
                )
            )
            .ToList();
    }

    /// <summary>
    /// Maps an <see cref="ArticleEntity" /> to its public projection from an already resolved
    /// cover URL. Performs no IO — batch mappings resolve files up front.
    /// </summary>
    public static PublicArticleSummaryDto ToPublicArticleSummaryDto(
        this ArticleEntity entity,
        ContentLookups lookups,
        string? coverImageUrl
    )
    {
        return new PublicArticleSummaryDto(
            entity.Id,
            entity.CategoryId,
            lookups.CategoryName(entity.CategoryId),
            entity.Title,
            entity.Slug,
            entity.Headline,
            coverImageUrl,
            entity.IsPromoted,
            entity.PublishedAt,
            entity.LikeCount,
            entity.CommentCount,
            entity.ShareCount,
            entity.BookmarkCount
        );
    }

    /// <summary>
    /// Maps a list of articles to their public card projection, cover URLs resolved in one
    /// batch.
    /// </summary>
    public static async Task<IReadOnlyList<PublicArticleSummaryDto>> ToPublicArticleSummaryDtosAsync(
        this IReadOnlyList<ArticleEntity> entities,
        ContentLookups lookups,
        IFileStorageService fileStorage,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> files = await fileStorage.ResolveManyAsync(
            entities.Where(e => e.CoverImageFileId.HasValue).Select(e => e.CoverImageFileId!.Value).Distinct().ToList(),
            ct
        );

        return entities
            .Select(entity =>
                entity.ToPublicArticleSummaryDto(
                    lookups,
                    coverImageUrl: entity.CoverImageFileId.HasValue
                        ? files.GetValueOrDefault(entity.CoverImageFileId.Value)?.StorageUrl
                        : null
                )
            )
            .ToList();
    }

    /// <summary>
    /// Maps a list of articles to their public card projection, stamping each with the current
    /// user's interaction flags. Pass empty sets for an anonymous request.
    /// </summary>
    public static async Task<IReadOnlyList<PublicArticleSummaryDto>> ToPublicArticleSummaryDtosAsync(
        this IReadOnlyList<ArticleEntity> entities,
        ContentLookups lookups,
        IFileStorageService fileStorage,
        IReadOnlySet<Guid> likedArticleIds,
        IReadOnlySet<Guid> bookmarkedArticleIds,
        CancellationToken ct = default
    )
    {
        IReadOnlyList<PublicArticleSummaryDto> summaries = await entities.ToPublicArticleSummaryDtosAsync(
            lookups,
            fileStorage,
            ct
        );

        return summaries
            .Select(dto =>
                dto with
                {
                    IsLiked = likedArticleIds.Contains(dto.Id),
                    IsBookmarked = bookmarkedArticleIds.Contains(dto.Id),
                }
            )
            .ToList();
    }

    /// <summary>
    /// Maps an <see cref="ArticleEntity" /> to its public card projection, resolving the cover
    /// image URL from the associated FileEntity.
    /// </summary>
    public static async Task<PublicArticleSummaryDto> ToPublicArticleSummaryDtoAsync(
        this ArticleEntity entity,
        ContentLookups lookups,
        IFileStorageService fileStorage,
        CancellationToken ct = default
    )
    {
        string? coverImageUrl = await ResolveCoverImageUrlAsync(entity, fileStorage, ct);
        return entity.ToPublicArticleSummaryDto(lookups, coverImageUrl: coverImageUrl);
    }

    /// <summary>
    /// Maps an <see cref="ArticleEntity" /> to its public card projection, resolving the cover
    /// URL and stamping the current user's interaction flags. Pass empty sets when anonymous.
    /// </summary>
    public static async Task<PublicArticleSummaryDto> ToPublicArticleSummaryDtoAsync(
        this ArticleEntity entity,
        ContentLookups lookups,
        IFileStorageService fileStorage,
        IReadOnlySet<Guid> likedArticleIds,
        IReadOnlySet<Guid> bookmarkedArticleIds,
        CancellationToken ct = default
    )
    {
        string? coverImageUrl = await ResolveCoverImageUrlAsync(entity, fileStorage, ct);

        return entity.ToPublicArticleSummaryDto(lookups, coverImageUrl: coverImageUrl) with
        {
            IsLiked = likedArticleIds.Contains(entity.Id),
            IsBookmarked = bookmarkedArticleIds.Contains(entity.Id),
        };
    }

    /// <summary>
    /// Maps an <see cref="ArticleEntity" /> to its public detail projection, resolving the
    /// cover image URL and stamping the current user's interaction flags.
    /// </summary>
    public static async Task<PublicArticleDetailDto> ToPublicArticleDetailDtoAsync(
        this ArticleEntity entity,
        IMapper mapper,
        ContentLookups lookups,
        IFileStorageService fileStorage,
        CancellationToken ct = default,
        bool isLiked = false,
        bool isBookmarked = false
    )
    {
        string? coverImageUrl = await ResolveCoverImageUrlAsync(entity, fileStorage, ct);

        return new PublicArticleDetailDto(
            entity.Id,
            entity.CategoryId,
            lookups.CategoryName(entity.CategoryId),
            entity.Title,
            entity.Slug,
            entity.Headline,
            entity.Body,
            coverImageUrl,
            entity.IsPromoted,
            entity.PublishedAt,
            entity.MetaTitle,
            entity.MetaDescription,
            mapper.Map<IReadOnlyList<ArticleImageDto>>(entity.Images),
            entity.TagDtos(mapper, lookups),
            Math.Max(
                1,
                (int)Math.Ceiling(entity.Body.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length / 200.0)
            ),
            entity.LikeCount,
            entity.CommentCount,
            entity.ShareCount,
            entity.BookmarkCount
        )
        {
            IsLiked = isLiked,
            IsBookmarked = isBookmarked,
        };
    }

    /// <summary>
    /// Maps an <see cref="ArticleCommentEntity" /> to its public projection. Deleted comments
    /// carry a null body.
    /// </summary>
    public static PublicArticleCommentDto ToPublicArticleCommentDto(this ArticleCommentEntity entity)
    {
        return new PublicArticleCommentDto(
            entity.Id,
            entity.UserId,
            entity.IsDeleted ? null : entity.Body,
            entity.IsDeleted,
            entity.CreatedAt,
            ParentCommentId: entity.ParentCommentId,
            LikeCount: entity.LikeCount
        );
    }

    /// <summary>
    /// Maps a list of <see cref="ArticleCommentEntity" /> to their public projection, attaching
    /// each commenter's resolved author profile. Deleted comments and commenters absent from
    /// <paramref name="authorsByUserId" /> carry a null author.
    /// </summary>
    public static IReadOnlyList<PublicArticleCommentDto> ToPublicArticleCommentDtos(
        this IReadOnlyList<ArticleCommentEntity> entities,
        IReadOnlyDictionary<Guid, PublicAuthorDto> authorsByUserId
    )
    {
        return entities
            .Select(entity =>
            {
                PublicArticleCommentDto dto = entity.ToPublicArticleCommentDto();

                if (entity.IsDeleted)
                {
                    return dto;
                }

                return dto with
                {
                    Author = authorsByUserId.GetValueOrDefault(entity.UserId),
                };
            })
            .ToList();
    }

    /// <summary>
    /// Resolves the cover image URL for an article.
    /// </summary>
    /// <remarks>
    /// Prefers the FileReferenceDto referenced by <c>CoverImageFileId</c>. Falls back to the
    /// <c>Cover</c> entry in the <c>Images</c> collection (when loaded) for covers that predate
    /// FileReferenceDto-backed tracking, where <c>CoverImageFileId</c> was never populated. Returns
    /// null when no cover image exists.
    /// </remarks>
    private static async Task<string?> ResolveCoverImageUrlAsync(
        ArticleEntity entity,
        IFileStorageService fileStorage,
        CancellationToken ct
    )
    {
        if (entity.CoverImageFileId.HasValue)
        {
            FileReferenceDto? coverFile = await fileStorage.ResolveAsync(entity.CoverImageFileId.Value, ct);
            if (coverFile?.StorageUrl is not null)
            {
                return coverFile.StorageUrl;
            }
        }

        return entity.Images?.FirstOrDefault(img => img.ImageType == EnumArticleImageType.Cover)?.Url;
    }

    /// <summary>
    /// Projects an article's tag junction rows through the resolved tag map, dropping any tag
    /// row that no longer exists.
    /// </summary>
    /// <param name="entity">The article whose tags to project.</param>
    /// <param name="mapper">Injected IMapper instance.</param>
    /// <param name="lookups">The resolved rows, including the tags.</param>
    /// <returns>The tag projections.</returns>
    private static IReadOnlyList<TagDto> TagDtos(this ArticleEntity entity, IMapper mapper, ContentLookups lookups)
    {
        return
        [
            .. entity
                .Tags.Select(articleTag => lookups.Tags.GetValueOrDefault(articleTag.TagId))
                .OfType<TagEntity>()
                .Select(mapper.Map<TagDto>),
        ];
    }
}
