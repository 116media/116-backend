using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Storage.Contracts.Application.Services;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArtist;

/// <summary>
/// Handles the <see cref="AdminUpdateArtistCommand" /> to update an artist profile's editable fields.
/// </summary>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="artistDtoService">Builds artist projections with their avatars resolved.</param>
/// <param name="timeProvider">Clock supplying today for the birthdate guard.</param>
public class AdminUpdateArtistHandler(
    IArtistRepository artistRepository,
    IContentUnitOfWork unitOfWork,
    IArtistDtoService artistDtoService,
    TimeProvider timeProvider
) : ICommandHandler<AdminUpdateArtistCommand, AdminUpdateArtistResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateArtistResult> Handle(
        AdminUpdateArtistCommand command,
        CancellationToken cancellationToken
    )
    {
        ArtistEntity artist = await artistRepository.GetByIdOrThrowAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        artist.Rename(name: command.Name);
        artist.ReviseProfile(
            bio: command.Bio,
            realName: command.RealName,
            aliases: command.Aliases,
            birthdate: command.Birthdate,
            hometown: command.Hometown,
            today: DateOnly.FromDateTime(dateTime: timeProvider.GetUtcNow().UtcDateTime)
        );
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = await artistDtoService.CreateAsync(artist, ct: cancellationToken);
        return new AdminUpdateArtistResult(Artist: dto);
    }
}
