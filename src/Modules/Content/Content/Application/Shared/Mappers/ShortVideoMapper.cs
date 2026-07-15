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
    /// The per-user <paramref name="isLiked" /> and <paramref name="isBookmarked" /> flags
    /// default to false so anonymous and listing callers omit them.
    /// </summary>
    public static async Task<ShortVideoDto> ToShortVideoDtoAsync(
        this ShortVideoEntity entity,
        IMapper mapper,
        IFileRepository fileRepository,
        CancellationToken ct = default,
        bool isLiked = false,
        bool isBookmarked = false
    )
    {
        var dto = mapper.Map<ShortVideoDto>(entity);

        string? videoUrl = null;
        if (entity.VideoFileId.HasValue)
        {
            FileEntity? videoFile = await fileRepository.GetByIdAsync(entity.VideoFileId.Value, ct);
            videoUrl = videoFile?.StorageUrl;
        }

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
            VideoSlug = entity.ParentVideo?.Slug,
            IsLiked = isLiked,
            IsBookmarked = isBookmarked,
        };
    }

    /// <summary>
    /// Maps a <see cref="ShortVideoEntity" /> to a <see cref="ShortVideoDto" />
    /// with the author profile and file URLs resolved, carrying the per-user
    /// like/bookmark flags through to the mapped DTO.
    /// </summary>
    public static async Task<ShortVideoDto> ToShortVideoDtoAsync(
        this ShortVideoEntity entity,
        IMapper mapper,
        IUserLookupService userLookup,
        IFileRepository fileRepository,
        CancellationToken ct = default,
        bool isLiked = false,
        bool isBookmarked = false
    )
    {
        ShortVideoDto dto = await entity.ToShortVideoDtoAsync(mapper, fileRepository, ct, isLiked, isBookmarked);

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
    /// Maps a <see cref="ShortVideoEntity" /> to a <see cref="ShortVideoDto" /> with no IO,
    /// resolving video/thumbnail/avatar URLs and the author profile from pre-fetched maps.
    /// Intended for batch mapping where files and authors are loaded once up front.
    /// </summary>
    public static ShortVideoDto ToShortVideoDto(
        this ShortVideoEntity entity,
        IMapper mapper,
        IReadOnlyDictionary<Guid, FileEntity> files,
        IReadOnlyDictionary<Guid, AuthorInfo> authors,
        IReadOnlySet<Guid> likedShortVideoIds,
        IReadOnlySet<Guid> bookmarkedShortVideoIds
    )
    {
        var dto = mapper.Map<ShortVideoDto>(entity);

        string? videoUrl =
            entity.VideoFileId is { } videoFileId && files.TryGetValue(videoFileId, out FileEntity? videoFile)
                ? videoFile.StorageUrl
                : null;

        string? thumbnailUrl;
        if (entity.ThumbnailFileId is { } thumbnailFileId && files.TryGetValue(thumbnailFileId, out FileEntity? thumb))
        {
            thumbnailUrl = thumb.StorageUrl;
        }
        else
        {
            thumbnailUrl = videoUrl is not null ? GenerateThumbnailUrl(videoUrl) : null;
        }

        AuthorDto? author = null;
        if (authors.TryGetValue(entity.AuthorId, out AuthorInfo? authorInfo))
        {
            string? avatarUrl =
                authorInfo.AvatarFileId is { } avatarFileId && files.TryGetValue(avatarFileId, out FileEntity? avatar)
                    ? avatar.StorageUrl
                    : null;

            author = new AuthorDto(
                UserName: authorInfo.UserName,
                Email: authorInfo.Email,
                AvatarUrl: avatarUrl,
                Role: authorInfo.Role
            );
        }

        return dto with
        {
            VideoUrl = videoUrl,
            ThumbnailUrl = thumbnailUrl,
            VideoSlug = entity.ParentVideo?.Slug,
            Author = author,
            IsLiked = likedShortVideoIds.Contains(entity.Id),
            IsBookmarked = bookmarkedShortVideoIds.Contains(entity.Id),
        };
    }

    /// <summary>
    /// Maps a list of short videos with file URLs resolved in a single batch (no author).
    /// </summary>
    public static Task<IReadOnlyList<ShortVideoDto>> ToShortVideoDtosAsync(
        this IReadOnlyList<ShortVideoEntity> entities,
        IMapper mapper,
        IFileRepository fileRepository,
        CancellationToken ct = default
    ) => entities.BuildDtosAsync(mapper, userLookup: null, fileRepository, EmptyIds, EmptyIds, ct);

    /// <summary>
    /// Maps a list of short videos with file URLs resolved in a single batch, stamping
    /// per-user like/bookmark flags from the supplied id sets (no author).
    /// </summary>
    public static Task<IReadOnlyList<ShortVideoDto>> ToShortVideoDtosAsync(
        this IReadOnlyList<ShortVideoEntity> entities,
        IMapper mapper,
        IFileRepository fileRepository,
        IReadOnlySet<Guid> likedShortVideoIds,
        IReadOnlySet<Guid> bookmarkedShortVideoIds,
        CancellationToken ct = default
    ) =>
        entities.BuildDtosAsync(
            mapper,
            userLookup: null,
            fileRepository,
            likedShortVideoIds,
            bookmarkedShortVideoIds,
            ct
        );

    /// <summary>
    /// Maps a list of short videos with author profiles and file URLs resolved in a single batch.
    /// </summary>
    public static Task<IReadOnlyList<ShortVideoDto>> ToShortVideoDtosAsync(
        this IReadOnlyList<ShortVideoEntity> entities,
        IMapper mapper,
        IUserLookupService userLookup,
        IFileRepository fileRepository,
        CancellationToken ct = default
    ) => entities.BuildDtosAsync(mapper, userLookup, fileRepository, EmptyIds, EmptyIds, ct);

    /// <summary>
    /// Maps a list of short videos with author profiles and file URLs resolved in a single
    /// batch, stamping per-user like/bookmark flags from the supplied id sets.
    /// </summary>
    public static Task<IReadOnlyList<ShortVideoDto>> ToShortVideoDtosAsync(
        this IReadOnlyList<ShortVideoEntity> entities,
        IMapper mapper,
        IUserLookupService userLookup,
        IFileRepository fileRepository,
        IReadOnlySet<Guid> likedShortVideoIds,
        IReadOnlySet<Guid> bookmarkedShortVideoIds,
        CancellationToken ct = default
    ) => entities.BuildDtosAsync(mapper, userLookup, fileRepository, likedShortVideoIds, bookmarkedShortVideoIds, ct);

    /// <summary>
    /// Batch-maps a list of short videos: resolves author profiles (deduped) in one query and
    /// all video/thumbnail/avatar file URLs in one query, then maps every entity in memory —
    /// two round-trips total, regardless of page size.
    /// </summary>
    private static async Task<IReadOnlyList<ShortVideoDto>> BuildDtosAsync(
        this IReadOnlyList<ShortVideoEntity> entities,
        IMapper mapper,
        IUserLookupService? userLookup,
        IFileRepository fileRepository,
        IReadOnlySet<Guid> likedShortVideoIds,
        IReadOnlySet<Guid> bookmarkedShortVideoIds,
        CancellationToken ct
    )
    {
        if (entities.Count == 0)
        {
            return [];
        }

        IReadOnlyDictionary<Guid, AuthorInfo> authors = EmptyAuthors;
        if (userLookup is not null)
        {
            List<Guid> authorIds = entities.Select(entity => entity.AuthorId).Distinct().ToList();
            authors = await userLookup.GetAuthorInfosByIdsAsync(authorIds, ct);
        }

        var fileIds = new HashSet<Guid>();
        foreach (ShortVideoEntity entity in entities)
        {
            if (entity.VideoFileId is { } videoFileId)
            {
                fileIds.Add(videoFileId);
            }
            if (entity.ThumbnailFileId is { } thumbnailFileId)
            {
                fileIds.Add(thumbnailFileId);
            }
        }
        foreach (AuthorInfo authorInfo in authors.Values)
        {
            if (authorInfo.AvatarFileId is { } avatarFileId)
            {
                fileIds.Add(avatarFileId);
            }
        }

        IReadOnlyDictionary<Guid, FileEntity> files =
            fileIds.Count == 0 ? EmptyFiles : await fileRepository.GetByIdsAsync(fileIds, ct);

        return entities
            .Select(entity =>
                entity.ToShortVideoDto(mapper, files, authors, likedShortVideoIds, bookmarkedShortVideoIds)
            )
            .ToList();
    }

    private static readonly IReadOnlySet<Guid> EmptyIds = new HashSet<Guid>();
    private static readonly IReadOnlyDictionary<Guid, AuthorInfo> EmptyAuthors = new Dictionary<Guid, AuthorInfo>();
    private static readonly IReadOnlyDictionary<Guid, FileEntity> EmptyFiles = new Dictionary<Guid, FileEntity>();

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
