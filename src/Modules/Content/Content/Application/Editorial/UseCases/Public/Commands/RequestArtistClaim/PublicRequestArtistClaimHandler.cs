using _116.Content.Application.Shared.Repositories;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.Extensions.Logging;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.RequestArtistClaim;

/// <summary>
/// Handles the <see cref="PublicRequestArtistClaimCommand" /> to record a request to claim
/// ownership of an artist profile. This phase has no dedicated claim-request entity or audit
/// log table, so the request is captured as a structured log entry for staff to review and
/// action manually via <see cref="_116.Content.Application.Editorial.UseCases.Admin.Commands.VerifyArtistOwner.AdminVerifyArtistOwnerCommand" />.
/// </summary>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="logger">Logger used to record the claim request for staff review.</param>
public class PublicRequestArtistClaimHandler(
    IArtistRepository artistRepository,
    ILogger<PublicRequestArtistClaimHandler> logger
) : ICommandHandler<PublicRequestArtistClaimCommand, PublicRequestArtistClaimResult>
{
    /// <inheritdoc />
    public async Task<PublicRequestArtistClaimResult> Handle(
        PublicRequestArtistClaimCommand command,
        CancellationToken cancellationToken
    )
    {
        await artistRepository.GetByIdOrThrowAsync(id: command.ArtistId, cancellationToken: cancellationToken);

        logger.LogInformation(
            "Artist claim requested: ArtistId={ArtistId}, UserId={UserId}, RequestedAt={RequestedAt}",
            command.ArtistId,
            command.UserId,
            DateTimeOffset.UtcNow
        );

        return new PublicRequestArtistClaimResult(IsSuccess: true);
    }
}
