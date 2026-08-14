using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertArtistSocialLink;

/// <summary>
/// Command to set or replace an artist's social link for a single platform. Upserts:
/// creates a new link row if none exists yet for the given artist and platform, otherwise
/// replaces the existing URL — one idempotent verb, so a form with one field per platform
/// never has to resolve a create-vs-update conflict.
/// </summary>
/// <param name="ArtistId">The artist profile this link belongs to.</param>
/// <param name="Platform">The social platform this link points to.</param>
/// <param name="Url">The outbound profile URL.</param>
public record AdminUpsertArtistSocialLinkCommand(Guid ArtistId, EnumSocialPlatform Platform, string Url)
    : ICommand<AdminUpsertArtistSocialLinkResult>;

/// <summary>
/// Result of the <see cref="AdminUpsertArtistSocialLinkCommand" /> containing the upserted link's id.
/// </summary>
/// <param name="SocialLinkId">The unique identifier of the upserted social link.</param>
public record AdminUpsertArtistSocialLinkResult(Guid SocialLinkId);
