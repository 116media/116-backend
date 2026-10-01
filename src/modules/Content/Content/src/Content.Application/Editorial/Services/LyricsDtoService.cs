using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Identity.Contracts.Application.Services;
using _116.Storage.Contracts.Application.Services;
using MapsterMapper;

namespace _116.Content.Application.Editorial.Services;

/// <summary>
/// Assembles lyrics response DTOs through the mapper extensions, owning the lookups, the author
/// profile and the file URL resolution.
/// </summary>
/// <param name="mapper">The Mapster mapper.</param>
/// <param name="userLookup">Service resolving author profiles from the Identity module.</param>
/// <param name="fileStorage">Storage contract resolving cover image and avatar URLs.</param>
/// <param name="contentLookupService">Resolver for the categories, customers and tags the DTOs name.</param>
public class LyricsDtoService(
    IMapper mapper,
    IUserLookupService userLookup,
    IFileStorageService fileStorage,
    IContentLookupService contentLookupService
) : ILyricsDtoService
{
    /// <inheritdoc />
    public async Task<LyricsDetailDto> CreateDetailAsync(LyricsEntity lyrics, CancellationToken ct = default)
    {
        ContentLookups lookups = await contentLookupService.ResolveForLyricsAsync([lyrics], ct);

        return await lyrics.ToLyricsDetailDtoAsync(lookups, mapper, userLookup, fileStorage, ct);
    }

    /// <inheritdoc />
    public async Task<PublicLyricsDetailDto> CreatePublicDetailAsync(
        LyricsEntity lyrics,
        bool isLiked = false,
        CancellationToken ct = default
    )
    {
        ContentLookups lookups = await contentLookupService.ResolveForLyricsAsync([lyrics], ct);

        return await lyrics.ToPublicLyricsDetailDtoAsync(lookups, mapper, userLookup, fileStorage, ct, isLiked);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PublicLyricsSummaryDto>> CreatePublicManyAsync(
        IReadOnlyList<LyricsEntity> lyrics,
        CancellationToken ct = default
    )
    {
        ContentLookups lookups = await contentLookupService.ResolveForLyricsAsync(lyrics, ct);

        return await lyrics.ToPublicLyricsSummaryDtosAsync(lookups, fileStorage, ct);
    }
}
