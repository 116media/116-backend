using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RemoveAlbumStreamingLink;

/// <summary>
/// Handles the <see cref="AdminRemoveAlbumStreamingLinkCommand" /> to remove an album's curated
/// streaming link for a single platform.
/// </summary>
/// <param name="streamingLinkRepository">Repository for streaming link data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminRemoveAlbumStreamingLinkHandler(
    IStreamingLinkRepository streamingLinkRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminRemoveAlbumStreamingLinkCommand, AdminRemoveAlbumStreamingLinkResult>
{
    /// <inheritdoc />
    public async Task<AdminRemoveAlbumStreamingLinkResult> Handle(
        AdminRemoveAlbumStreamingLinkCommand command,
        CancellationToken cancellationToken
    )
    {
        StreamingLinkEntity? existing = await streamingLinkRepository.GetByAlbumAndPlatformAsync(
            albumId: command.AlbumId,
            platform: command.Platform,
            cancellationToken: cancellationToken
        );

        if (existing is null)
        {
            return new AdminRemoveAlbumStreamingLinkResult(IsSuccess: true);
        }

        streamingLinkRepository.Remove(streamingLink: existing);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminRemoveAlbumStreamingLinkResult(IsSuccess: true);
    }
}
