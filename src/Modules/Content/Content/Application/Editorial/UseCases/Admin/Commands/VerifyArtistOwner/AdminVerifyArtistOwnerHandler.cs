using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.Services;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.VerifyArtistOwner;

/// <summary>
/// Handles the <see cref="AdminVerifyArtistOwnerCommand" /> to confirm and finalize an artist
/// profile's ownership claim.
/// </summary>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="fileStorage">Core's storage contract.</param>
public class AdminVerifyArtistOwnerHandler(
    IArtistRepository artistRepository,
    IContentUnitOfWork unitOfWork,
    IFileStorageService fileStorage
) : ICommandHandler<AdminVerifyArtistOwnerCommand, AdminVerifyArtistOwnerResult>
{
    /// <inheritdoc />
    public async Task<AdminVerifyArtistOwnerResult> Handle(
        AdminVerifyArtistOwnerCommand command,
        CancellationToken cancellationToken
    )
    {
        ArtistEntity artist = await artistRepository.GetByIdOrThrowAsync(
            id: command.ArtistId,
            cancellationToken: cancellationToken
        );

        artist.ClaimOwnership(userId: command.UserId);

        artistRepository.Update(artist: artist);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = await artist.ToArtistDtoAsync(fileStorage, cancellationToken);
        return new AdminVerifyArtistOwnerResult(Artist: dto);
    }
}
