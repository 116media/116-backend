using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for ContentType entity mappings.
/// Uses dependency injection instead of global static state.
/// </summary>
public static class ContentTypeMapper
{
    /// <summary>
    /// Registers ContentType entity mappings into the provided TypeAdapterConfig.
    /// This method does NOT mutate global state.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ContentTypeEntity, ContentTypeDto>();
    }

    /// <summary>
    /// Maps a <see cref="ContentTypeEntity" /> to a <see cref="ContentTypeDto" />.
    /// </summary>
    /// <param name="entity">The content type entity to map.</param>
    /// <param name="mapper">Injected IMapper instance.</param>
    /// <returns>A <see cref="ContentTypeDto" /> containing content type information.</returns>
    public static ContentTypeDto ToContentTypeDto(this ContentTypeEntity entity, IMapper mapper)
    {
        return mapper.Map<ContentTypeDto>(entity);
    }

    /// <summary>
    /// Maps a collection of <see cref="ContentTypeEntity" /> to a list of <see cref="ContentTypeDto" />.
    /// </summary>
    /// <param name="entities">The content type entities to map.</param>
    /// <param name="mapper">Injected IMapper instance.</param>
    /// <returns>A read-only list of <see cref="ContentTypeDto" />.</returns>
    public static IReadOnlyList<ContentTypeDto> ToContentTypeDtos(
        this IReadOnlyList<ContentTypeEntity> entities,
        IMapper mapper
    )
    {
        return mapper.Map<IReadOnlyList<ContentTypeDto>>(entities);
    }
}
