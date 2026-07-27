using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks;

/// <summary>
/// Command to resolve an album's streaming links from one verified platform URL. The
/// external provider fans the source URL out to every platform it can match, and each match
/// is upserted as a curated link — one paste instead of one manual entry per platform.
/// </summary>
/// <param name="AlbumId">The album whose streaming links are being resolved.</param>
/// <param name="SourceUrl">A verified album URL on any supported platform.</param>
public record AdminResolveAlbumStreamingLinksCommand(Guid AlbumId, string SourceUrl)
    : ICommand<AdminResolveAlbumStreamingLinksResult>;

/// <summary>
/// Result of the <see cref="AdminResolveAlbumStreamingLinksCommand" />, reporting what
/// happened per platform so the admin UI can show it.
/// </summary>
/// <param name="Resolved">Platforms whose deep links were stored or replaced.</param>
/// <param name="Unresolved">
/// Modelled platforms the provider had no link for — their generated search-URL fallback
/// keeps serving, and any existing curated row is left untouched.
/// </param>
public record AdminResolveAlbumStreamingLinksResult(
    IReadOnlyList<EnumStreamingPlatform> Resolved,
    IReadOnlyList<EnumStreamingPlatform> Unresolved
);
