using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Contracts.Application;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration and extension methods for ShortVideo entity mappings.
/// Video and thumbnail URLs are resolved from associated FileEntity records
/// rather than stored as flat strings on the entity.
/// </summary>
public static class ShortVideoMapper
{
    /// <summary>
    /// Registers ShortVideo entity mappings into the provided TypeAdapterConfig.
    /// Ignores <c>VideoUrl</c> and <c>ThumbnailUrl</c> since they are resolved at mapping time
    /// from associated FileEntity records.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<ShortVideoEntity, ShortVideoDto>()
            .Map(dest => dest.VideoUrl, _ => (string?)null)
            .Map(dest => dest.ThumbnailUrl, _ => (string?)null);
    }

    /// <summary>
    /// Maps a <see cref="ShortVideoEntity" /> to a <see cref="ShortVideoDto" />,
    /// resolving the video and thumbnail URLs from associated FileEntity records.
    /// </summary>
    public static async Task<ShortVideoDto> ToShortVideoDtoAsync(
        this ShortVideoEntity entity,
        IMapper mapper,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        var dto = mapper.Map<ShortVideoDto>(entity);

        FileEntity? videoFile = await fileRepository.GetByIdAsync(entity.VideoFileId, ct);
        string? videoUrl = videoFile?.StorageUrl;

        string? thumbnailUrl = null;
        if (entity.ThumbnailFileId.HasValue)
        {
            FileEntity? thumbnailFile = await fileRepository.GetByIdAsync(entity.ThumbnailFileId.Value, ct);
            thumbnailUrl = thumbnailFile?.StorageUrl;
        }
        else if (videoUrl is not null)
        {
            thumbnailUrl = GenerateThumbnailUrl(videoUrl);
        }

        return dto with
        {
            VideoUrl = videoUrl,
            ThumbnailUrl = thumbnailUrl,
        };
    }

    /// <summary>
    /// Maps a <see cref="ShortVideoEntity" /> to a <see cref="ShortVideoDto" />
    /// with the author profile and file URLs resolved.
    /// </summary>
    public static async Task<ShortVideoDto> ToShortVideoDtoAsync(
        this ShortVideoEntity entity,
        IMapper mapper,
        IUserLookupService userLookup,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        ShortVideoDto dto = await entity.ToShortVideoDtoAsync(mapper, fileRepository, ct);

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
    /// Maps a list of <see cref="ShortVideoEntity" /> to a list of <see cref="ShortVideoDto" />
    /// with file URLs resolved from associated FileEntity records.
    /// </summary>
    public static async Task<IReadOnlyList<ShortVideoDto>> ToShortVideoDtosAsync(
        this IReadOnlyList<ShortVideoEntity> entities,
        IMapper mapper,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        var results = new List<ShortVideoDto>(entities.Count);
        foreach (ShortVideoEntity entity in entities)
        {
            results.Add(await entity.ToShortVideoDtoAsync(mapper, fileRepository, ct));
        }
        return results;
    }

    /// <summary>
    /// Maps a list of <see cref="ShortVideoEntity" /> to a list of <see cref="ShortVideoDto" />
    /// with author profiles and file URLs resolved.
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

    /// <summary>
    /// Generates an auto-thumbnail URL from a Cloudinary video URL by inserting
    /// a screenshot-at-1-second transformation and changing the extension to JPG.
    /// Used when no manual thumbnail has been uploaded.
    /// </summary>
    private static string GenerateThumbnailUrl(string videoUrl)
    {
        string jpgUrl = Path.ChangeExtension(videoUrl, ".jpg");
        return jpgUrl.Replace("/video/upload/", "/video/upload/so_1,q_auto,f_auto,w_720/");
    }
}
