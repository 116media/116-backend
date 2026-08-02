using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RemoveAlbumStreamingLink;

/// <summary>
/// Command to remove an album's curated streaming link for a single platform, reverting that
/// platform's public link back to the generated search-query fallback. A no-op if no curated
/// link exists for the given album and platform.
/// </summary>
/// <param name="AlbumId">The album this link belongs to.</param>
/// <param name="Platform">The streaming platform whose curated link is being removed.</param>
public record AdminRemoveAlbumStreamingLinkCommand(Guid AlbumId, EnumStreamingPlatform Platform)
    : ICommand<AdminRemoveAlbumStreamingLinkResult>;

/// <summary>
/// Result of the <see cref="AdminRemoveAlbumStreamingLinkCommand" /> indicating whether a curated link was removed.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation completed successfully.</param>
public record AdminRemoveAlbumStreamingLinkResult(bool IsSuccess);
