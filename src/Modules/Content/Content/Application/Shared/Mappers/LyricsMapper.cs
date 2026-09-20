using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for Lyrics entity mappings.
/// </summary>
/// <remarks>
/// <c>LyricsEntity → LyricsSummaryDto</c> and <c>LyricsEntity → LyricsDetailDto</c> are handled
/// as plain C# in the extension methods below rather than registered Mapster configs, mirroring
/// <see cref="ArticleMapper" />, since both DTOs pull <c>CategoryName</c>/<c>CustomerName</c> off
/// navigation properties that may be null and resolve the author profile from the Identity module.
/// </remarks>
public static class LyricsMapper
{
    /// <summary>
    /// Registers Lyrics-tag entity mappings into the provided TypeAdapterConfig.
    /// </summary>
    /// <param name="config">
    /// The TypeAdapterConfig to register mappings into.
    /// </param>
    public static void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<TagEntity, TagDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Slug, src => src.Slug);
    }

    /// <summary>
    /// Maps a <see cref="LyricsEntity" /> to a <see cref="LyricsSummaryDto" />,
    /// resolving the cover image URL from the associated FileEntity. <c>IsLiked</c> always
    /// resolves to false — use the overload taking <paramref name="likedLyricsIds" /> below to
    /// stamp per-caller interaction state.
    /// </summary>
    public static async Task<LyricsSummaryDto> ToLyricsSummaryDtoAsync(
        this LyricsEntity entity,
        ContentLookups lookups,
        IFileStorageService fileStorage,
        CancellationToken ct = default
    )
    {
        string? coverImageUrl = await ResolveCoverImageUrlAsync(entity, fileStorage, ct);

        return entity.ToLyricsSummaryDto(lookups, coverImageUrl: coverImageUrl);
    }

    /// <summary>
    /// Maps a <see cref="LyricsEntity" /> to a <see cref="LyricsSummaryDto" /> from an
    /// already resolved cover URL. Performs no IO — batch mappings resolve files up front.
    /// </summary>
    public static LyricsSummaryDto ToLyricsSummaryDto(
        this LyricsEntity entity,
        ContentLookups lookups,
        string? coverImageUrl
    )
    {
        return new LyricsSummaryDto(
            entity.Id,
            entity.CategoryId,
            lookups.CategoryName(entity.CategoryId),
            entity.SongTitle,
            entity.ArtistName,
            entity.Slug,
            entity.Language,
            entity.VideoId,
            coverImageUrl,
            entity.AuthorId.ToString(),
            entity.Status,
            entity.PublishedAt,
            entity.ViewCount,
            entity.LikeCount,
            entity.ShareCount
        )
        {
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
        };
    }

    /// <summary>
    /// Maps a list of <see cref="LyricsEntity" /> to a list of <see cref="LyricsSummaryDto" />,
    /// resolving cover image URLs from associated FileReferenceDto records. <c>IsLiked</c> always
    /// resolves to false on every item — use the overload taking
    /// <paramref name="likedLyricsIds" /> below to stamp per-caller interaction state.
    /// </summary>
    public static async Task<IReadOnlyList<LyricsSummaryDto>> ToLyricsSummaryDtosAsync(
        this IReadOnlyList<LyricsEntity> entities,
        ContentLookups lookups,
        IFileStorageService fileStorage,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> files = await fileStorage.ResolveManyAsync(
            entities.Where(e => e.CoverImageFileId.HasValue).Select(e => e.CoverImageFileId!.Value).Distinct().ToList(),
            ct
        );

        return entities
            .Select(entity =>
                entity.ToLyricsSummaryDto(
                    lookups,
                    coverImageUrl: entity.CoverImageFileId.HasValue
                        ? files.GetValueOrDefault(entity.CoverImageFileId.Value)?.StorageUrl
                        : null
                )
            )
            .ToList();
    }

    /// <summary>
    /// Maps a <see cref="LyricsEntity" /> to a <see cref="LyricsSummaryDto" />, stamping the
    /// current user's <c>IsLiked</c> flag from the supplied liked-ids set. Pass an empty set
    /// for an anonymous request or an admin context that does not need per-user state.
    /// </summary>
    /// <param name="entity">The lyrics page to map.</param>
    /// <param name="fileStorage">Core's storage contract.</param>
    /// <param name="likedLyricsIds">Ids the current user has liked.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The mapped summary with the interaction flag applied.</returns>
    public static async Task<LyricsSummaryDto> ToLyricsSummaryDtoAsync(
        this LyricsEntity entity,
        ContentLookups lookups,
        IFileStorageService fileStorage,
        IReadOnlySet<Guid> likedLyricsIds,
        CancellationToken ct = default
    )
    {
        LyricsSummaryDto dto = await entity.ToLyricsSummaryDtoAsync(lookups, fileStorage, ct);
        return dto with { IsLiked = likedLyricsIds.Contains(entity.Id) };
    }

    /// <summary>
    /// Maps a list of lyrics pages to summaries, stamping each with the current user's
    /// <c>IsLiked</c> flag from the supplied liked-ids set. Pass an empty set for an
    /// anonymous request or an admin context that does not need per-user state.
    /// </summary>
    /// <param name="entities">The lyrics pages to map.</param>
    /// <param name="fileStorage">Core's storage contract.</param>
    /// <param name="likedLyricsIds">Ids the current user has liked.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The mapped summaries with the interaction flag applied.</returns>
    public static async Task<IReadOnlyList<LyricsSummaryDto>> ToLyricsSummaryDtosAsync(
        this IReadOnlyList<LyricsEntity> entities,
        ContentLookups lookups,
        IFileStorageService fileStorage,
        IReadOnlySet<Guid> likedLyricsIds,
        CancellationToken ct = default
    )
    {
        IReadOnlyList<LyricsSummaryDto> summaries = await entities.ToLyricsSummaryDtosAsync(lookups, fileStorage, ct);

        return summaries.Select(dto => dto with { IsLiked = likedLyricsIds.Contains(dto.Id) }).ToList();
    }

    /// <summary>
    /// Maps a <see cref="LyricsEntity" /> to a <see cref="LyricsDetailDto" />,
    /// resolving the cover image URL from the associated FileReferenceDto and the author profile
    /// from the Identity module.
    /// </summary>
    /// <param name="entity">The lyrics page to map.</param>
    /// <param name="mapper">The Mapster mapper used for tags.</param>
    /// <param name="userLookup">Service for resolving author profiles from the Identity module.</param>
    /// <param name="fileStorage">Core's storage contract.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <param name="isLiked">
    /// Whether the current user has liked this lyrics page. False when anonymous.
    /// </param>
    /// <returns>The mapped detail DTO.</returns>
    public static async Task<LyricsDetailDto> ToLyricsDetailDtoAsync(
        this LyricsEntity entity,
        ContentLookups lookups,
        IMapper mapper,
        IUserLookupService userLookup,
        IFileStorageService fileStorage,
        CancellationToken ct = default,
        bool isLiked = false
    )
    {
        string? coverImageUrl = await ResolveCoverImageUrlAsync(entity, fileStorage, ct);

        var dto = new LyricsDetailDto(
            entity.Id,
            entity.CategoryId,
            lookups.CategoryName(entity.CategoryId),
            entity.SongTitle,
            entity.ArtistName,
            entity.Slug,
            entity.LyricsText,
            entity.Language,
            entity.VideoId,
            entity.Status,
            entity.RejectionReason,
            entity.PublishedAt,
            entity.MetaTitle,
            entity.MetaDescription,
            coverImageUrl,
            entity.Album,
            entity.ReleaseYear,
            entity.Label,
            entity.Songwriter,
            entity.Producer,
            entity.TagDtos(mapper, lookups),
            entity.AuthorId.ToString(),
            entity.ViewCount,
            entity.LikeCount,
            entity.ShareCount,
            entity.CustomerId,
            lookups.CustomerName(entity.CustomerId),
            entity.OrderItemId
        )
        {
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
            IsLiked = isLiked,
        };

        AuthorDto? authorInfo = await userLookup.GetAuthorInfoByIdAsync(userId: entity.AuthorId, ct: ct);

        if (authorInfo is null)
        {
            return dto;
        }

        string? avatarUrl = null;
        if (authorInfo.AvatarFileId.HasValue)
        {
            FileReferenceDto? avatarFile = await fileStorage.ResolveAsync(authorInfo.AvatarFileId.Value, ct);
            avatarUrl = avatarFile?.StorageUrl;
        }

        return dto with
        {
            Author = new AdminAuthorDto(
                UserName: authorInfo.UserName,
                Email: authorInfo.Email,
                AvatarUrl: avatarUrl,
                Role: authorInfo.Role
            ),
        };
    }

    /// <summary>
    /// Maps a <see cref="LyricsEntity" /> to its public card projection from an already
    /// resolved cover URL. Performs no IO — batch mappings resolve files up front.
    /// </summary>
    public static PublicLyricsSummaryDto ToPublicLyricsSummaryDto(
        this LyricsEntity entity,
        ContentLookups lookups,
        string? coverImageUrl
    )
    {
        return new PublicLyricsSummaryDto(
            entity.Id,
            entity.CategoryId,
            lookups.CategoryName(entity.CategoryId),
            entity.SongTitle,
            entity.ArtistName,
            entity.Slug,
            entity.Language,
            entity.VideoId,
            coverImageUrl,
            entity.PublishedAt,
            entity.ViewCount,
            entity.LikeCount,
            entity.ShareCount
        );
    }

    /// <summary>
    /// Maps a list of lyrics to their public card projection, cover URLs resolved in one
    /// batch. Pass empty ids for an anonymous request.
    /// </summary>
    public static async Task<IReadOnlyList<PublicLyricsSummaryDto>> ToPublicLyricsSummaryDtosAsync(
        this IReadOnlyList<LyricsEntity> entities,
        ContentLookups lookups,
        IFileStorageService fileStorage,
        IReadOnlySet<Guid> likedLyricsIds,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> files = await fileStorage.ResolveManyAsync(
            entities.Where(e => e.CoverImageFileId.HasValue).Select(e => e.CoverImageFileId!.Value).Distinct().ToList(),
            ct
        );

        return entities
            .Select(entity =>
                entity.ToPublicLyricsSummaryDto(
                    lookups,
                    coverImageUrl: entity.CoverImageFileId.HasValue
                        ? files.GetValueOrDefault(entity.CoverImageFileId.Value)?.StorageUrl
                        : null
                ) with
                {
                    IsLiked = likedLyricsIds.Contains(entity.Id),
                }
            )
            .ToList();
    }

    /// <summary>
    /// Maps a list of lyrics to their public card projection, cover URLs resolved in one
    /// batch. <c>IsLiked</c> stays false on every item — anonymous requests.
    /// </summary>
    public static async Task<IReadOnlyList<PublicLyricsSummaryDto>> ToPublicLyricsSummaryDtosAsync(
        this IReadOnlyList<LyricsEntity> entities,
        ContentLookups lookups,
        IFileStorageService fileStorage,
        CancellationToken ct = default
    )
    {
        return await entities.ToPublicLyricsSummaryDtosAsync(
            lookups,
            fileStorage,
            likedLyricsIds: new HashSet<Guid>(),
            ct
        );
    }

    /// <summary>
    /// Maps a <see cref="LyricsEntity" /> to its public detail projection, resolving the cover
    /// URL and the author's public profile from the Identity module.
    /// </summary>
    public static async Task<PublicLyricsDetailDto> ToPublicLyricsDetailDtoAsync(
        this LyricsEntity entity,
        ContentLookups lookups,
        IMapper mapper,
        IUserLookupService userLookup,
        IFileStorageService fileStorage,
        CancellationToken ct = default,
        bool isLiked = false
    )
    {
        string? coverImageUrl = await ResolveCoverImageUrlAsync(entity, fileStorage, ct);

        var dto = new PublicLyricsDetailDto(
            entity.Id,
            entity.CategoryId,
            lookups.CategoryName(entity.CategoryId),
            entity.SongTitle,
            entity.ArtistName,
            entity.Slug,
            entity.LyricsText,
            entity.Language,
            entity.VideoId,
            entity.PublishedAt,
            entity.MetaTitle,
            entity.MetaDescription,
            coverImageUrl,
            entity.Album,
            entity.ReleaseYear,
            entity.Label,
            entity.Songwriter,
            entity.Producer,
            entity.TagDtos(mapper, lookups),
            entity.ViewCount,
            entity.LikeCount,
            entity.ShareCount
        )
        {
            IsLiked = isLiked,
        };

        AuthorDto? authorInfo = await userLookup.GetAuthorInfoByIdAsync(userId: entity.AuthorId, ct: ct);

        if (authorInfo is null)
        {
            return dto;
        }

        string? avatarUrl = null;
        if (authorInfo.AvatarFileId.HasValue)
        {
            FileReferenceDto? avatarFile = await fileStorage.ResolveAsync(authorInfo.AvatarFileId.Value, ct);
            avatarUrl = avatarFile?.StorageUrl;
        }

        return dto with
        {
            Author = new PublicAuthorDto(UserName: authorInfo.UserName, AvatarUrl: avatarUrl),
        };
    }

    /// <summary>
    /// Resolves the cover image URL for a lyrics page. Returns null when no cover has been
    /// uploaded, mirroring <see cref="ArticleMapper" />'s equivalent resolution helper.
    /// </summary>
    private static async Task<string?> ResolveCoverImageUrlAsync(
        LyricsEntity entity,
        IFileStorageService fileStorage,
        CancellationToken ct
    )
    {
        if (!entity.CoverImageFileId.HasValue)
        {
            return null;
        }

        FileReferenceDto? coverFile = await fileStorage.ResolveAsync(entity.CoverImageFileId.Value, ct);
        return coverFile?.StorageUrl;
    }

    /// <summary>
    /// Projects a lyrics page's tag junction rows through the resolved tag map, dropping any
    /// tag row that no longer exists.
    /// </summary>
    /// <param name="entity">The lyrics page whose tags to project.</param>
    /// <param name="mapper">Injected IMapper instance.</param>
    /// <param name="lookups">The resolved rows, including the tags.</param>
    /// <returns>The tag projections.</returns>
    private static IReadOnlyList<TagDto> TagDtos(this LyricsEntity entity, IMapper mapper, ContentLookups lookups)
    {
        return
        [
            .. entity
                .Tags.Select(lyricsTag => lookups.Tags.GetValueOrDefault(lyricsTag.TagId))
                .OfType<TagEntity>()
                .Select(mapper.Map<TagDto>),
        ];
    }
}
