using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.VerifyArtistOwner;

/// <summary>
/// Command for an admin to confirm and finalize an artist profile's ownership claim,
/// linking the profile to the verified identity user.
/// </summary>
/// <param name="ArtistId">The artist profile being verified.</param>
/// <param name="UserId">The identity user UUID confirmed as the profile's owner.</param>
public record AdminVerifyArtistOwnerCommand(Guid ArtistId, Guid UserId) : ICommand<AdminVerifyArtistOwnerResult>;

/// <summary>
/// Result of the <see cref="AdminVerifyArtistOwnerCommand" /> containing the now-claimed artist profile.
/// </summary>
/// <param name="Artist">The claimed artist profile information.</param>
public record AdminVerifyArtistOwnerResult(ArtistDto Artist);
