using _116.Content.Application.Interactions.Persistence;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.CreatePlaylist;

/// <summary>
/// Handles the <see cref="PublicCreatePlaylistCommand" /> to create a new user playlist.
/// </summary>
/// <param name="playlistRepository">Repository for playlist data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicCreatePlaylistHandler(IPlaylistRepository playlistRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<PublicCreatePlaylistCommand, PublicCreatePlaylistResult>
{
    /// <inheritdoc />
    public async Task<PublicCreatePlaylistResult> Handle(
        PublicCreatePlaylistCommand command,
        CancellationToken cancellationToken
    )
    {
        var playlist = PlaylistEntity.Create(id: Guid.NewGuid(), userId: command.UserId, name: command.Name);

        await playlistRepository.AddAsync(playlist: playlist, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = new PlaylistDto(Id: playlist.Id, Name: playlist.Name, VideoCount: 0);
        return new PublicCreatePlaylistResult(Playlist: dto);
    }
}
