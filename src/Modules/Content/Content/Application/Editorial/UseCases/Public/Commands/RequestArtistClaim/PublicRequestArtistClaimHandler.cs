using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.RequestArtistClaim;

/// <summary>
/// Handles the <see cref="PublicRequestArtistClaimCommand" /> to record a request to claim
/// ownership of an artist profile. Persists a durable
/// <see cref="ArtistClaimRequestEntity" /> row for staff to review and action manually via
/// <see cref="_116.Content.Application.Editorial.UseCases.Admin.Commands.VerifyArtistOwner.AdminVerifyArtistOwnerCommand" />;
/// the artist profile itself is never mutated here.
/// A profile that already has a verified owner accepts no further claims, and one account files
/// at most one request per profile — the queue holds requests worth reviewing, not a submit-count.
/// </summary>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="claimRequestRepository">Repository for artist claim request data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicRequestArtistClaimHandler(
    IArtistRepository artistRepository,
    IArtistClaimRequestRepository claimRequestRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<PublicRequestArtistClaimCommand, PublicRequestArtistClaimResult>
{
    /// <inheritdoc />
    public async Task<PublicRequestArtistClaimResult> Handle(
        PublicRequestArtistClaimCommand command,
        CancellationToken cancellationToken
    )
    {
        ArtistEntity artist = await artistRepository.GetByIdOrThrowAsync(
            id: command.ArtistId,
            cancellationToken: cancellationToken
        );

        if (artist.UserId.HasValue)
        {
            throw i18n.Artist.AlreadyClaimed();
        }

        bool alreadyRequested = await claimRequestRepository.ExistsForArtistAndUserAsync(
            artistId: command.ArtistId,
            userId: command.UserId,
            cancellationToken: cancellationToken
        );

        if (alreadyRequested)
        {
            throw i18n.Artist.ClaimRequestAlreadyExists();
        }

        var claimRequest = ArtistClaimRequestEntity.Create(
            id: Guid.NewGuid(),
            artistId: command.ArtistId,
            userId: command.UserId
        );

        await claimRequestRepository.AddAsync(claimRequest: claimRequest, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicRequestArtistClaimResult(IsSuccess: true);
    }
}
