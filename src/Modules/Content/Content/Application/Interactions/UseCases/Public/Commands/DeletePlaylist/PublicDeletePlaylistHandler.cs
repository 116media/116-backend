using _116.Content.Application.Interactions.Persistence;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.DeletePlaylist;

/// <summary>
/// Handles the <see cref="PublicDeletePlaylistCommand" /> to delete a user playlist.
/// </summary>
/// <param name="playlistRepository">Repository for playlist data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicDeletePlaylistHandler(IPlaylistRepository playlistRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<PublicDeletePlaylistCommand, PublicDeletePlaylistResult>
{
    /// <inheritdoc />
    public async Task<PublicDeletePlaylistResult> Handle(
        PublicDeletePlaylistCommand command,
        CancellationToken cancellationToken
    )
    {
        PlaylistEntity? playlist = await playlistRepository.GetByIdAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        if (playlist is null)
        {
            throw PlaylistErrors.NotFound(id: command.Id);
        }

        if (playlist.UserId != command.UserId)
        {
            throw PlaylistErrors.NotOwner();
        }

        playlistRepository.Delete(playlist: playlist);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicDeletePlaylistResult(IsSuccess: true);
    }
}
