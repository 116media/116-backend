using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Contracts.Application;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for Lyrics entity mappings.
/// </summary>
public static class LyricsMapper
{
    /// <summary>
    /// Registers Lyrics entity mappings into the provided TypeAdapterConfig.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<LyricsEntity, LyricsDto>();
    }

    /// <summary>
    /// Maps a <see cref="LyricsEntity" /> to a <see cref="LyricsDto" />.
    /// </summary>
    public static LyricsDto ToLyricsDto(this LyricsEntity entity, IMapper mapper)
    {
        return mapper.Map<LyricsDto>(entity);
    }

    /// <summary>
    /// Maps a <see cref="LyricsEntity" /> to a <see cref="LyricsDto" />
    /// with the author profile resolved from the Identity module.
    /// </summary>
    public static async Task<LyricsDto> ToLyricsDtoAsync(
        this LyricsEntity entity,
        IMapper mapper,
        IUserLookupService userLookup,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        var dto = mapper.Map<LyricsDto>(entity);

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
    /// Maps a list of <see cref="LyricsEntity" /> to a list of <see cref="LyricsDto" />.
    /// </summary>
    public static IReadOnlyList<LyricsDto> ToLyricsDtos(this IReadOnlyList<LyricsEntity> entities, IMapper mapper)
    {
        return mapper.Map<IReadOnlyList<LyricsDto>>(entities);
    }

    /// <summary>
    /// Maps a list of <see cref="LyricsEntity" /> to a list of <see cref="LyricsDto" />
    /// with author profiles resolved from the Identity module.
    /// </summary>
    public static async Task<IReadOnlyList<LyricsDto>> ToLyricsDtosAsync(
        this IReadOnlyList<LyricsEntity> entities,
        IMapper mapper,
        IUserLookupService userLookup,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        var results = new List<LyricsDto>(entities.Count);
        foreach (LyricsEntity entity in entities)
        {
            results.Add(await entity.ToLyricsDtoAsync(mapper, userLookup, fileRepository, ct));
        }
        return results;
    }
}
