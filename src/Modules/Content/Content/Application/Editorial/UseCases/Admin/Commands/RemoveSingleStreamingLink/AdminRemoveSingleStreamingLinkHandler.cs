using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RemoveSingleStreamingLink;

/// <summary>
/// Handles the <see cref="AdminRemoveSingleStreamingLinkCommand" /> to remove a standalone
/// single's curated streaming link for a single platform.
/// </summary>
/// <param name="streamingLinkRepository">Repository for streaming link data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminRemoveSingleStreamingLinkHandler(
    IStreamingLinkRepository streamingLinkRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminRemoveSingleStreamingLinkCommand, AdminRemoveSingleStreamingLinkResult>
{
    /// <inheritdoc />
    public async Task<AdminRemoveSingleStreamingLinkResult> Handle(
        AdminRemoveSingleStreamingLinkCommand command,
        CancellationToken cancellationToken
    )
    {
        StreamingLinkEntity? existing = await streamingLinkRepository.GetByLyricsAndPlatformAsync(
            lyricsId: command.LyricsId,
            platform: command.Platform,
            cancellationToken: cancellationToken
        );

        if (existing is null)
        {
            return new AdminRemoveSingleStreamingLinkResult(IsSuccess: true);
        }

        streamingLinkRepository.Remove(streamingLink: existing);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminRemoveSingleStreamingLinkResult(IsSuccess: true);
    }
}
