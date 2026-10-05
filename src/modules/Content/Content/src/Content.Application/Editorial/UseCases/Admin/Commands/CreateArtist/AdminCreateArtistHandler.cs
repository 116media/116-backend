using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist.Contracts;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist;

/// <summary>
/// Handles the <see cref="AdminCreateArtistCommand" /> to create an artist profile.
/// </summary>
/// <param name="createArtistService">Service resolving and staging the artist.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="artistDtoService">Service assembling the artist DTO.</param>
public class AdminCreateArtistHandler(
    IAdminCreateArtistService createArtistService,
    IContentUnitOfWork unitOfWork,
    IArtistDtoService artistDtoService
) : ICommandHandler<AdminCreateArtistCommand, AdminCreateArtistResult>
{
    /// <inheritdoc />
    public async Task<AdminCreateArtistResult> Handle(
        AdminCreateArtistCommand command,
        CancellationToken cancellationToken
    )
    {
        ArtistEntity artist = await createArtistService.CreateAsync(command, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = await artistDtoService.CreateAsync(artist, ct: cancellationToken);
        return new AdminCreateArtistResult(Artist: dto);
    }
}
