using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertAlbumStreamingLink;

/// <summary>
/// Command to set or replace an album's curated streaming link for a single platform.
/// Upserts: creates a new streaming link row if none exists yet for the given album and
/// platform, otherwise replaces the existing curated URL.
/// </summary>
/// <param name="AlbumId">The album this link belongs to.</param>
/// <param name="Platform">The streaming platform this link points to.</param>
/// <param name="Url">The curated deep link URL.</param>
public record AdminUpsertAlbumStreamingLinkCommand(Guid AlbumId, EnumStreamingPlatform Platform, string Url)
    : ICommand<AdminUpsertAlbumStreamingLinkResult>;

/// <summary>
/// Result of the <see cref="AdminUpsertAlbumStreamingLinkCommand" /> containing the upserted streaming link's id.
/// </summary>
/// <param name="StreamingLinkId">The unique identifier of the upserted streaming link.</param>
public record AdminUpsertAlbumStreamingLinkResult(Guid StreamingLinkId);
