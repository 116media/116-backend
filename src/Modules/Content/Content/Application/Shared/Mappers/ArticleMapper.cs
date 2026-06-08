using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
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
    /// resolving the cover image URL from the associated FileEntity.
    /// </summary>
    public static async Task<ArticleDetailDto> ToArticleDetailDtoAsync(
        this ArticleEntity entity,
        IMapper mapper,
        IFileRepository fileRepository,
        CancellationToken ct = default
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
    /// Resolves the cover image URL from the associated FileEntity, or returns null
    /// if no cover image has been uploaded.
    /// </summary>
    private static async Task<string?> ResolveCoverImageUrlAsync(
        ArticleEntity entity,
        IFileRepository fileRepository,
        CancellationToken ct
    )
    {
        if (!entity.CoverImageFileId.HasValue)
        {
            return null;
        }

        FileEntity? coverFile = await fileRepository.GetByIdAsync(entity.CoverImageFileId.Value, ct);
        return coverFile?.StorageUrl;
    }
}
