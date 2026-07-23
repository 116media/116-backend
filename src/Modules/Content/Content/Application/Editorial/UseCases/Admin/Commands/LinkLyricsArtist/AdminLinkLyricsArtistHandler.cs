using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.LinkLyricsArtist;

/// <summary>
/// Handles the <see cref="AdminLinkLyricsArtistCommand" /> to link or unlink a lyrics page's
/// real, addressable artist profile.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminLinkLyricsArtistHandler(
    ILyricsRepository lyricsRepository,
    IArtistRepository artistRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminLinkLyricsArtistCommand, AdminLinkLyricsArtistResult>
{
    /// <inheritdoc />
    public async Task<AdminLinkLyricsArtistResult> Handle(
        AdminLinkLyricsArtistCommand command,
        CancellationToken cancellationToken
    )
    {
        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(
            id: command.LyricsId,
            cancellationToken: cancellationToken
        );

        if (command.ArtistId.HasValue)
        {
            await artistRepository.GetByIdOrThrowAsync(
                id: command.ArtistId.Value,
                cancellationToken: cancellationToken
            );
            lyrics.LinkArtist(artistId: command.ArtistId.Value);
        }
        else
        {
            lyrics.UnlinkArtist();
        }

        lyricsRepository.Update(lyrics: lyrics);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminLinkLyricsArtistResult(IsSuccess: true);
    }
}
