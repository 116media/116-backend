using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.LinkVideoArtist;

/// <summary>
/// Handles the <see cref="AdminLinkVideoArtistCommand" /> to link or unlink a video's real,
/// addressable artist profile.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminLinkVideoArtistHandler(
    IVideoRepository videoRepository,
    IArtistRepository artistRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminLinkVideoArtistCommand, AdminLinkVideoArtistResult>
{
    /// <inheritdoc />
    public async Task<AdminLinkVideoArtistResult> Handle(
        AdminLinkVideoArtistCommand command,
        CancellationToken cancellationToken
    )
    {
        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(
            id: command.VideoId,
            cancellationToken: cancellationToken
        );

        if (command.ArtistId.HasValue)
        {
            await artistRepository.GetByIdOrThrowAsync(
                id: command.ArtistId.Value,
                cancellationToken: cancellationToken
            );
            video.LinkArtist(artistId: command.ArtistId.Value);
        }
        else
        {
            video.UnlinkArtist();
        }

        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminLinkVideoArtistResult(IsSuccess: true);
    }
}
