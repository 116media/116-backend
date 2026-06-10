using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for Category and CategoryPricing entity mappings.
/// Poster URL is resolved from the associated FileEntity at mapping time
/// rather than stored as a flat string on the entity.
/// </summary>
public static class CategoryMapper
{
    /// <summary>
    /// Registers Category and CategoryPricing entity mappings into the provided TypeAdapterConfig.
    /// Ignores <c>PosterUrl</c> since it is resolved at mapping time from the associated FileEntity.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<CategoryPricingEntity, CategoryPricingDto>()
            .Map(dest => dest.TierId, src => src.PricingTierId)
            .Map(dest => dest.TierName, src => src.PricingTier.Name)
            .Map(dest => dest.PriceUsd, src => src.PriceUsd);

        config
            .NewConfig<CategoryEntity, CategoryDto>()
            .Map(dest => dest.ContentTypeName, src => src.ContentType.Name)
            .Map(dest => dest.PosterUrl, _ => (string?)null)
            .Map(dest => dest.Pricing, src => src.Pricing);
    }

    /// <summary>
    /// Maps a <see cref="CategoryEntity" /> to a <see cref="CategoryDto" />,
    /// resolving the poster URL from the associated FileEntity record.
    /// </summary>
    public static async Task<CategoryDto> ToCategoryDtoAsync(
        this CategoryEntity entity,
        IMapper mapper,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        var dto = mapper.Map<CategoryDto>(entity);

        string? posterUrl = null;
        if (entity.PosterFileId.HasValue)
        {
            FileEntity? posterFile = await fileRepository.GetByIdAsync(entity.PosterFileId.Value, ct);
            posterUrl = posterFile?.StorageUrl;
        }

        return dto with
        {
            PosterUrl = posterUrl,
            Pricing = mapper.Map<IReadOnlyList<CategoryPricingDto>>(entity.Pricing),
        };
    }

    /// <summary>
    /// Maps a collection of <see cref="CategoryEntity" /> to a list of <see cref="CategoryDto" />,
    /// resolving poster URLs from associated FileEntity records.
    /// </summary>
    public static async Task<IReadOnlyList<CategoryDto>> ToCategoryDtosAsync(
        this IReadOnlyList<CategoryEntity> entities,
        IMapper mapper,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        var results = new List<CategoryDto>(entities.Count);
        foreach (CategoryEntity entity in entities)
        {
            results.Add(await entity.ToCategoryDtoAsync(mapper, fileRepository, ct));
        }

        return results;
    }

    /// <summary>
    /// Maps a <see cref="CategoryPricingEntity" /> to a <see cref="CategoryPricingDto" />.
    /// </summary>
    public static CategoryPricingDto ToCategoryPricingDto(this CategoryPricingEntity entity, IMapper mapper)
    {
        return mapper.Map<CategoryPricingDto>(entity);
    }
}
