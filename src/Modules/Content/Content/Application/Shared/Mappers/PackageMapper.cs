using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for Package and PackageSlot entity mappings.
/// </summary>
public static class PackageMapper
{
    /// <summary>
    /// Registers Package and PackageSlot entity mappings into the provided TypeAdapterConfig.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<PackageSlotEntity, PackageSlotDto>()
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : null);

        config.NewConfig<PackageEntity, PackageDto>().Map(dest => dest.Slots, src => src.Slots);
    }

    /// <summary>Maps a <see cref="PackageEntity" /> to a <see cref="PackageDto" />.</summary>
    public static PackageDto ToPackageDto(this PackageEntity entity, IMapper mapper)
    {
        return mapper.Map<PackageDto>(entity);
    }

    /// <summary>Maps a collection of <see cref="PackageEntity" /> to a list of <see cref="PackageDto" />.</summary>
    public static IReadOnlyList<PackageDto> ToPackageDtos(this IReadOnlyList<PackageEntity> entities, IMapper mapper)
    {
        return mapper.Map<IReadOnlyList<PackageDto>>(entities);
    }

    /// <summary>Maps a <see cref="PackageSlotEntity" /> to a <see cref="PackageSlotDto" />.</summary>
    public static PackageSlotDto ToPackageSlotDto(this PackageSlotEntity entity, IMapper mapper)
    {
        return mapper.Map<PackageSlotDto>(entity);
    }
}
