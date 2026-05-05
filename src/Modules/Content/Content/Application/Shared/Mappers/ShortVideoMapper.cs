using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Contracts.Application;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for ShortVideo entity mappings.
/// </summary>
public static class ShortVideoMapper
{
    /// <summary>
    /// Registers ShortVideo entity mappings into the provided TypeAdapterConfig.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ShortVideoEntity, ShortVideoDto>();
    }

    /// <summary>
    /// Maps a <see cref="ShortVideoEntity" /> to a <see cref="ShortVideoDto" />.
    /// </summary>
    public static ShortVideoDto ToShortVideoDto(this ShortVideoEntity entity, IMapper mapper)
    {
        return mapper.Map<ShortVideoDto>(entity);
    }

    /// <summary>
    /// Maps a <see cref="ShortVideoEntity" /> to a <see cref="ShortVideoDto" />
    /// with the author profile resolved from the Identity module.
    /// </summary>
    public static async Task<ShortVideoDto> ToShortVideoDtoAsync(
        this ShortVideoEntity entity,
        IMapper mapper,
        IUserLookupService userLookup,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        var dto = mapper.Map<ShortVideoDto>(entity);

        AuthorInfo? authorInfo = await userLookup.GetAuthorInfoByIdAsync(userId: entity.AuthorId, ct: ct);

        if (authorInfo is null)
        {
            return dto;
        }

        string? avatarUrl = null;
        if (authorInfo.AvatarFileId.HasValue)
        {
            FileEntity? avatarFile = await fileRepository.GetByIdAsync(authorInfo.AvatarFileId.Value, ct);
            avatarUrl = avatarFile?.StorageUrl;
        }

        return dto with
        {
            Author = new AuthorDto(
                UserName: authorInfo.UserName,
                Email: authorInfo.Email,
                AvatarUrl: avatarUrl,
                Role: authorInfo.Role
            ),
        };
    }

    /// <summary>
    /// Maps a list of <see cref="ShortVideoEntity" /> to a list of <see cref="ShortVideoDto" />.
    /// </summary>
    public static IReadOnlyList<ShortVideoDto> ToShortVideoDtos(
        this IReadOnlyList<ShortVideoEntity> entities,
        IMapper mapper
    )
    {
        return mapper.Map<IReadOnlyList<ShortVideoDto>>(entities);
    }

    /// <summary>
    /// Maps a list of <see cref="ShortVideoEntity" /> to a list of <see cref="ShortVideoDto" />
    /// with author profiles resolved from the Identity module.
    /// </summary>
    public static async Task<IReadOnlyList<ShortVideoDto>> ToShortVideoDtosAsync(
        this IReadOnlyList<ShortVideoEntity> entities,
        IMapper mapper,
        IUserLookupService userLookup,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        var results = new List<ShortVideoDto>(entities.Count);
        foreach (ShortVideoEntity entity in entities)
        {
            results.Add(await entity.ToShortVideoDtoAsync(mapper, userLookup, fileRepository, ct));
        }
        return results;
    }
}
