using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertAlbumStreamingLink;

/// <summary>
/// Handles the <see cref="AdminUpsertAlbumStreamingLinkCommand" /> to set or replace an album's
/// curated streaming link for a single platform.
/// </summary>
/// <param name="albumRepository">Repository for album data access operations.</param>
/// <param name="streamingLinkRepository">Repository for streaming link data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUpsertAlbumStreamingLinkHandler(
    IAlbumRepository albumRepository,
    IStreamingLinkRepository streamingLinkRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminUpsertAlbumStreamingLinkCommand, AdminUpsertAlbumStreamingLinkResult>
{
    /// <inheritdoc />
    public async Task<AdminUpsertAlbumStreamingLinkResult> Handle(
        AdminUpsertAlbumStreamingLinkCommand command,
        CancellationToken cancellationToken
    )
    {
        await albumRepository.GetByIdOrThrowAsync(id: command.AlbumId, cancellationToken: cancellationToken);

        StreamingLinkEntity? existing = await streamingLinkRepository.GetByAlbumAndPlatformAsync(
            albumId: command.AlbumId,
            platform: command.Platform,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            existing.UpdateUrl(url: command.Url);
            streamingLinkRepository.Update(streamingLink: existing);
            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

            return new AdminUpsertAlbumStreamingLinkResult(StreamingLinkId: existing.Id);
        }

        StreamingLinkEntity streamingLink = StreamingLinkEntity.ForAlbum(
            id: Guid.NewGuid(),
            albumId: command.AlbumId,
            platform: command.Platform,
            url: command.Url
        );

        await streamingLinkRepository.AddAsync(streamingLink: streamingLink, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminUpsertAlbumStreamingLinkResult(StreamingLinkId: streamingLink.Id);
    }
}
