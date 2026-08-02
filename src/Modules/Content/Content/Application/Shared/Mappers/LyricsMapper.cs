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
            .NewConfig<LyricsTagEntity, TagDto>()
            .Map(dest => dest.Id, src => src.Tag.Id)
            .Map(dest => dest.Name, src => src.Tag.Name)
            .Map(dest => dest.Slug, src => src.Tag.Slug);
    }

    /// <summary>
    /// Maps a <see cref="LyricsEntity" /> to a <see cref="LyricsSummaryDto" />,
    /// resolving the cover image URL from the associated FileEntity. <c>IsLiked</c> always
    /// resolves to false — use the overload taking <paramref name="likedLyricsIds" /> below to
    /// stamp per-caller interaction state.
    /// </summary>
    public static async Task<LyricsSummaryDto> ToLyricsSummaryDtoAsync(
        this LyricsEntity entity,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        string? coverImageUrl = await ResolveCoverImageUrlAsync(entity, fileRepository, ct);

        return new LyricsSummaryDto(
            entity.Id,
            entity.CategoryId,
            entity.Category != null ? entity.Category.Name : string.Empty,
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
    /// resolving cover image URLs from associated FileEntity records. <c>IsLiked</c> always
    /// resolves to false on every item — use the overload taking
    /// <paramref name="likedLyricsIds" /> below to stamp per-caller interaction state.
    /// </summary>
    public static async Task<IReadOnlyList<LyricsSummaryDto>> ToLyricsSummaryDtosAsync(
        this IReadOnlyList<LyricsEntity> entities,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        var results = new List<LyricsSummaryDto>(entities.Count);
        foreach (LyricsEntity entity in entities)
        {
            results.Add(await entity.ToLyricsSummaryDtoAsync(fileRepository, ct));
        }
        return results;
    }

    /// <summary>
    /// Maps a <see cref="LyricsEntity" /> to a <see cref="LyricsSummaryDto" />, stamping the
    /// current user's <c>IsLiked</c> flag from the supplied liked-ids set. Pass an empty set
    /// for an anonymous request or an admin context that does not need per-user state.
    /// </summary>
    /// <param name="entity">The lyrics page to map.</param>
    /// <param name="fileRepository">Repository used to resolve the cover image URL.</param>
    /// <param name="likedLyricsIds">Ids the current user has liked.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The mapped summary with the interaction flag applied.</returns>
    public static async Task<LyricsSummaryDto> ToLyricsSummaryDtoAsync(
        this LyricsEntity entity,
        IFileRepository fileRepository,
        IReadOnlySet<Guid> likedLyricsIds,
        CancellationToken ct = default
    )
    {
        LyricsSummaryDto dto = await entity.ToLyricsSummaryDtoAsync(fileRepository, ct);
        return dto with { IsLiked = likedLyricsIds.Contains(entity.Id) };
    }

    /// <summary>
    /// Maps a list of lyrics pages to summaries, stamping each with the current user's
    /// <c>IsLiked</c> flag from the supplied liked-ids set. Pass an empty set for an
    /// anonymous request or an admin context that does not need per-user state.
    /// </summary>
    /// <param name="entities">The lyrics pages to map.</param>
    /// <param name="fileRepository">Repository used to resolve cover image URLs.</param>
    /// <param name="likedLyricsIds">Ids the current user has liked.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The mapped summaries with the interaction flag applied.</returns>
    public static async Task<IReadOnlyList<LyricsSummaryDto>> ToLyricsSummaryDtosAsync(
        this IReadOnlyList<LyricsEntity> entities,
        IFileRepository fileRepository,
        IReadOnlySet<Guid> likedLyricsIds,
        CancellationToken ct = default
    )
    {
        var results = new List<LyricsSummaryDto>(entities.Count);
        foreach (LyricsEntity entity in entities)
        {
            results.Add(await entity.ToLyricsSummaryDtoAsync(fileRepository, likedLyricsIds, ct));
        }
        return results;
    }

    /// <summary>
    /// Maps a <see cref="LyricsEntity" /> to a <see cref="LyricsDetailDto" />,
    /// resolving the cover image URL from the associated FileEntity and the author profile
    /// from the Identity module.
    /// </summary>
    /// <param name="entity">The lyrics page to map.</param>
    /// <param name="mapper">The Mapster mapper used for tags.</param>
    /// <param name="userLookup">Service for resolving author profiles from the Identity module.</param>
    /// <param name="fileRepository">Repository used to resolve the cover image URL.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <param name="isLiked">
    /// Whether the current user has liked this lyrics page. False when anonymous.
    /// </param>
    /// <returns>The mapped detail DTO.</returns>
    public static async Task<LyricsDetailDto> ToLyricsDetailDtoAsync(
        this LyricsEntity entity,
        IMapper mapper,
        IUserLookupService userLookup,
        IFileRepository fileRepository,
        CancellationToken ct = default,
        bool isLiked = false
    )
    {
        string? coverImageUrl = await ResolveCoverImageUrlAsync(entity, fileRepository, ct);

        var dto = new LyricsDetailDto(
            entity.Id,
            entity.CategoryId,
            entity.Category != null ? entity.Category.Name : string.Empty,
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
            mapper.Map<IReadOnlyList<TagDto>>(entity.Tags),
            entity.AuthorId.ToString(),
            entity.ViewCount,
            entity.LikeCount,
            entity.ShareCount,
            entity.CustomerId,
            entity.Customer != null ? entity.Customer.FullName : null,
            entity.OrderItemId
        )
        {
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
            IsLiked = isLiked,
        };

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
    /// Maps a list of <see cref="LyricsEntity" /> to a list of <see cref="LyricsDetailDto" />,
    /// resolving each cover image URL and author profile from the Identity module.
    /// </summary>
    public static async Task<IReadOnlyList<LyricsDetailDto>> ToLyricsDetailDtosAsync(
        this IReadOnlyList<LyricsEntity> entities,
        IMapper mapper,
        IUserLookupService userLookup,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        var results = new List<LyricsDetailDto>(entities.Count);
        foreach (LyricsEntity entity in entities)
        {
            results.Add(await entity.ToLyricsDetailDtoAsync(mapper, userLookup, fileRepository, ct));
        }
        return results;
    }

    /// <summary>
    /// Resolves the cover image URL for a lyrics page. Returns null when no cover has been
    /// uploaded, mirroring <see cref="ArticleMapper" />'s equivalent resolution helper.
    /// </summary>
    private static async Task<string?> ResolveCoverImageUrlAsync(
        LyricsEntity entity,
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
