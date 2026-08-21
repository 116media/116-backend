using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.VerifyArtistOwner;

/// <summary>
/// Handles the <see cref="AdminVerifyArtistOwnerCommand" /> to confirm and finalize an artist
/// profile's ownership claim.
/// </summary>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="fileRepository">Repository for resolving avatar file URLs.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminVerifyArtistOwnerHandler(
    IArtistRepository artistRepository,
    IContentUnitOfWork unitOfWork,
    IFileRepository fileRepository,
    ContentI18n i18n
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

        var dto = await artist.ToArtistDtoAsync(fileRepository, cancellationToken);
        return new AdminVerifyArtistOwnerResult(Artist: dto);
    }
}
