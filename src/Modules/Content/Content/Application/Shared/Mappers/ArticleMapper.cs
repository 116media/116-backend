using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
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
            .NewConfig<ArticleTagEntity, TagDto>()
            .Map(dest => dest.Id, src => src.Tag.Id)
            .Map(dest => dest.Name, src => src.Tag.Name)
            .Map(dest => dest.Slug, src => src.Tag.Slug);

        config
            .NewConfig<ArticleCommentEntity, ArticleCommentDto>()
            .Map(dest => dest.Body, src => src.IsDeleted ? null : src.Body);
    }

    /// <summary>
    /// Maps an <see cref="ArticleEntity" /> to an <see cref="ArticleSummaryDto" />,
    /// resolving the cover image URL from the associated FileEntity.
    /// </summary>
    public static async Task<ArticleSummaryDto> ToArticleSummaryDtoAsync(
        this ArticleEntity entity,
        IMapper mapper,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        string? coverImageUrl = await ResolveCoverImageUrlAsync(entity, fileRepository, ct);

        return new ArticleSummaryDto(
            entity.Id,
            entity.CategoryId,
            entity.Category != null ? entity.Category.Name : string.Empty,
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
    /// resolving the cover image URL from the associated FileEntity and stamping the
    /// current user's interaction flags.
    /// </summary>
    /// <param name="entity">The article to map.</param>
    /// <param name="mapper">The Mapster mapper used for images and tags.</param>
    /// <param name="fileRepository">Repository used to resolve the cover image URL.</param>
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
        IFileRepository fileRepository,
        CancellationToken ct = default,
        bool isLiked = false,
        bool isBookmarked = false
    )
    {
        string? coverImageUrl = await ResolveCoverImageUrlAsync(entity, fileRepository, ct);

        return new ArticleDetailDto(
            entity.Id,
            entity.CategoryId,
            entity.Category != null ? entity.Category.Name : string.Empty,
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
            entity.PromotionLevel?.Name,
            entity.PublishedAt,
            entity.MetaTitle,
            entity.MetaDescription,
            mapper.Map<IReadOnlyList<ArticleImageDto>>(entity.Images),
            mapper.Map<IReadOnlyList<TagDto>>(entity.Tags),
            Math.Max(
                1,
                (int)Math.Ceiling(entity.Body.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length / 200.0)
            ),
            entity.LikeCount,
            entity.CommentCount,
            entity.ShareCount,
            entity.BookmarkCount,
            entity.CustomerId,
            entity.Customer != null ? entity.Customer.FullName : null,
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
    /// resolving cover image URLs from associated FileEntity records.
    /// </summary>
    public static async Task<IReadOnlyList<ArticleSummaryDto>> ToArticleSummaryDtosAsync(
        this IReadOnlyList<ArticleEntity> entities,
        IMapper mapper,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        var results = new List<ArticleSummaryDto>(entities.Count);
        foreach (ArticleEntity entity in entities)
        {
            results.Add(await entity.ToArticleSummaryDtoAsync(mapper, fileRepository, ct));
        }
        return results;
    }

    /// <summary>
    /// Maps an <see cref="ArticleEntity" /> to an <see cref="ArticleSummaryDto" />, stamping the
    /// current user's interaction flags from the supplied liked/bookmarked id sets. Pass empty
    /// sets for an anonymous request.
    /// </summary>
    /// <param name="entity">The article to map.</param>
    /// <param name="mapper">The Mapster mapper.</param>
    /// <param name="fileRepository">Repository used to resolve the cover image URL.</param>
    /// <param name="likedArticleIds">Ids the current user has liked.</param>
    /// <param name="bookmarkedArticleIds">Ids the current user has bookmarked.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The mapped summary with interaction flags applied.</returns>
    public static async Task<ArticleSummaryDto> ToArticleSummaryDtoAsync(
        this ArticleEntity entity,
        IMapper mapper,
        IFileRepository fileRepository,
        IReadOnlySet<Guid> likedArticleIds,
        IReadOnlySet<Guid> bookmarkedArticleIds,
        CancellationToken ct = default
    )
    {
        ArticleSummaryDto dto = await entity.ToArticleSummaryDtoAsync(mapper, fileRepository, ct);
        return dto with
        {
            IsLiked = likedArticleIds.Contains(entity.Id),
            IsBookmarked = bookmarkedArticleIds.Contains(entity.Id),
        };
    }

    /// <summary>
    /// Maps a list of articles to summaries, stamping each with the current user's interaction
    /// flags from the supplied liked/bookmarked id sets. Pass empty sets for an anonymous request.
    /// </summary>
    /// <param name="entities">The articles to map.</param>
    /// <param name="mapper">The Mapster mapper.</param>
    /// <param name="fileRepository">Repository used to resolve cover image URLs.</param>
    /// <param name="likedArticleIds">Ids the current user has liked.</param>
    /// <param name="bookmarkedArticleIds">Ids the current user has bookmarked.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The mapped summaries with interaction flags applied.</returns>
    public static async Task<IReadOnlyList<ArticleSummaryDto>> ToArticleSummaryDtosAsync(
        this IReadOnlyList<ArticleEntity> entities,
        IMapper mapper,
        IFileRepository fileRepository,
        IReadOnlySet<Guid> likedArticleIds,
        IReadOnlySet<Guid> bookmarkedArticleIds,
        CancellationToken ct = default
    )
    {
        var results = new List<ArticleSummaryDto>(entities.Count);
        foreach (ArticleEntity entity in entities)
        {
            results.Add(
                await entity.ToArticleSummaryDtoAsync(mapper, fileRepository, likedArticleIds, bookmarkedArticleIds, ct)
            );
        }
        return results;
    }

    /// <summary>
    /// Maps an <see cref="ArticleCommentEntity" /> to an <see cref="ArticleCommentDto" />.
    /// </summary>
    public static ArticleCommentDto ToArticleCommentDto(this ArticleCommentEntity entity, IMapper mapper)
    {
        var dto = mapper.Map<ArticleCommentDto>(entity);
        return dto with { Body = entity.IsDeleted ? null : entity.Body };
    }

    /// <summary>
    /// Maps a list of <see cref="ArticleCommentEntity" /> to a list of <see cref="ArticleCommentDto" />.
    /// </summary>
    public static IReadOnlyList<ArticleCommentDto> ToArticleCommentDtos(
        this IReadOnlyList<ArticleCommentEntity> entities,
        IMapper mapper
    )
    {
        return entities.Select(e => e.ToArticleCommentDto(mapper)).ToList();
    }

    /// <summary>
    /// Resolves the cover image URL for an article.
    /// </summary>
    /// <remarks>
    /// Prefers the FileEntity referenced by <c>CoverImageFileId</c>. Falls back to the
    /// <c>Cover</c> entry in the <c>Images</c> collection (when loaded) for covers that predate
    /// FileEntity-backed tracking, where <c>CoverImageFileId</c> was never populated. Returns
    /// null when no cover image exists.
    /// </remarks>
    private static async Task<string?> ResolveCoverImageUrlAsync(
        ArticleEntity entity,
        IFileRepository fileRepository,
        CancellationToken ct
    )
    {
        if (entity.CoverImageFileId.HasValue)
        {
            FileEntity? coverFile = await fileRepository.GetByIdAsync(entity.CoverImageFileId.Value, ct);
            if (coverFile?.StorageUrl is not null)
            {
                return coverFile.StorageUrl;
            }
        }

        return entity.Images?.FirstOrDefault(img => img.ImageType == EnumArticleImageType.Cover)?.Url;
    }
}
