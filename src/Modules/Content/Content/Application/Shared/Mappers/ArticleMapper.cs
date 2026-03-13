using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
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
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<ArticleImageEntity, ArticleImageDto>()
            .Map(dest => dest.ImageType, src => src.ImageType.ToString());

        config
            .NewConfig<ArticleTagEntity, TagDto>()
            .Map(dest => dest.Id, src => src.Tag.Id)
            .Map(dest => dest.Name, src => src.Tag.Name)
            .Map(dest => dest.Slug, src => src.Tag.Slug);

        config
            .NewConfig<ArticleEntity, ArticleSummaryDto>()
            .Map(dest => dest.CategoryName, src => src.Category.Name)
            .Map(dest => dest.Status, src => src.Status.ToString());

        config
            .NewConfig<ArticleEntity, ArticleDetailDto>()
            .Map(dest => dest.CategoryName, src => src.Category.Name)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Images, src => src.Images)
            .Map(dest => dest.Tags, src => src.Tags)
            .Map(
                dest => dest.ReadTimeInMinutes,
                src =>
                    Math.Max(
                        1,
                        (int)Math.Ceiling(src.Body.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length / 200.0)
                    )
            );
    }

    /// <summary>
    /// Maps an <see cref="ArticleEntity" /> to an <see cref="ArticleSummaryDto" />.
    /// </summary>
    public static ArticleSummaryDto ToArticleSummaryDto(this ArticleEntity entity, IMapper mapper)
    {
        return mapper.Map<ArticleSummaryDto>(entity);
    }

    /// <summary>
    /// Maps an <see cref="ArticleEntity" /> to an <see cref="ArticleDetailDto" />.
    /// </summary>
    public static ArticleDetailDto ToArticleDetailDto(this ArticleEntity entity, IMapper mapper)
    {
        return mapper.Map<ArticleDetailDto>(entity);
    }

    /// <summary>
    /// Maps a list of <see cref="ArticleEntity" /> to a list of <see cref="ArticleSummaryDto" />.
    /// </summary>
    public static IReadOnlyList<ArticleSummaryDto> ToArticleSummaryDtos(
        this IReadOnlyList<ArticleEntity> entities,
        IMapper mapper
    )
    {
        return mapper.Map<IReadOnlyList<ArticleSummaryDto>>(entities);
    }
}
