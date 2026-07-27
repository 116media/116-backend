using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RemoveArtistSocialLink;

/// <summary>
/// Command to remove an artist's social link for a single platform. Removing a platform
/// that has no link is a 404, not a silent success — the admin asked to delete something
/// specific and should learn it was not there.
/// </summary>
/// <param name="ArtistId">The artist profile the link belongs to.</param>
/// <param name="Platform">The platform slot to remove.</param>
public record AdminRemoveArtistSocialLinkCommand(Guid ArtistId, EnumSocialPlatform Platform)
    : ICommand<AdminRemoveArtistSocialLinkResult>;

/// <summary>
/// Result of the <see cref="AdminRemoveArtistSocialLinkCommand" />.
/// </summary>
/// <param name="IsSuccess">Whether the link was removed.</param>
public record AdminRemoveArtistSocialLinkResult(bool IsSuccess);
