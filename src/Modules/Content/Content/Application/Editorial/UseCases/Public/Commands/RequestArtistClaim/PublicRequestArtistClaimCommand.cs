using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.RequestArtistClaim;

/// <summary>
/// Command to record a request to claim ownership of an artist profile. This does not grant
/// ownership by itself — it only logs the request for staff to review. An admin must confirm
/// the claim separately via
/// <see cref="_116.Content.Application.Editorial.UseCases.Admin.Commands.VerifyArtistOwner.AdminVerifyArtistOwnerCommand" />
/// before the profile's <c>UserId</c> is actually set.
/// </summary>
/// <param name="ArtistId">The artist profile being claimed.</param>
/// <param name="UserId">The identity user UUID read from JWT claims.</param>
public record PublicRequestArtistClaimCommand(Guid ArtistId, Guid UserId) : ICommand<PublicRequestArtistClaimResult>;

/// <summary>
/// Result of the <see cref="PublicRequestArtistClaimCommand" /> indicating the claim request was recorded.
/// </summary>
/// <param name="IsSuccess">Indicates if the request was recorded successfully.</param>
public record PublicRequestArtistClaimResult(bool IsSuccess);
