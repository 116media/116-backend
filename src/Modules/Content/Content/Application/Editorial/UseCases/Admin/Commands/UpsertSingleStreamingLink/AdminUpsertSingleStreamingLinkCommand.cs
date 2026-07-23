using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertSingleStreamingLink;

/// <summary>
/// Command to set or replace a standalone single's curated streaming link for a single
/// platform. Rejected when the target lyrics page belongs to an album — a track that
/// belongs to an album gets its streaming links through the album, not per-track.
/// Upserts: creates a new streaming link row if none exists yet for the given lyrics page and
/// platform, otherwise replaces the existing curated URL.
/// </summary>
/// <param name="LyricsId">The standalone single (lyrics page) this link belongs to.</param>
/// <param name="Platform">The streaming platform this link points to.</param>
/// <param name="Url">The curated deep link URL.</param>
public record AdminUpsertSingleStreamingLinkCommand(Guid LyricsId, EnumStreamingPlatform Platform, string Url)
    : ICommand<AdminUpsertSingleStreamingLinkResult>;

/// <summary>
/// Result of the <see cref="AdminUpsertSingleStreamingLinkCommand" /> containing the upserted streaming link's id.
/// </summary>
/// <param name="StreamingLinkId">The unique identifier of the upserted streaming link.</param>
public record AdminUpsertSingleStreamingLinkResult(Guid StreamingLinkId);
