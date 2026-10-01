using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks.Contracts;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks;

/// <summary>
/// Handles the <see cref="AdminResolveAlbumStreamingLinksCommand" /> to resolve and store an album's
/// streaming links.
/// </summary>
/// <param name="linkResolutionService">Service gating the album and resolving the links.</param>
/// <param name="streamingLinkRepository">Repository storing the resolved links.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminResolveAlbumStreamingLinksHandler(
    IAdminAlbumLinkResolutionService linkResolutionService,
    IStreamingLinkRepository streamingLinkRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminResolveAlbumStreamingLinksCommand, AdminResolveAlbumStreamingLinksResult>
{
    /// <inheritdoc />
    public async Task<AdminResolveAlbumStreamingLinksResult> Handle(
        AdminResolveAlbumStreamingLinksCommand command,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyDictionary<EnumStreamingPlatform, string> resolved = await linkResolutionService.ResolveAsync(
            albumId: command.AlbumId,
            sourceUrl: command.SourceUrl,
            cancellationToken: cancellationToken
        );

        foreach ((EnumStreamingPlatform platform, string url) in resolved)
        {
            StreamingLinkEntity? existing = await streamingLinkRepository.GetByAlbumAndPlatformAsync(
                albumId: command.AlbumId,
                platform: platform,
                cancellationToken: cancellationToken
            );

            if (existing is not null)
            {
                existing.UpdateUrl(url: url);
                continue;
            }

            StreamingLinkEntity streamingLink = StreamingLinkEntity.ForAlbum(
                id: Guid.NewGuid(),
                albumId: command.AlbumId,
                platform: platform,
                url: url
            );
            await streamingLinkRepository.AddAsync(streamingLink: streamingLink, cancellationToken: cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        List<EnumStreamingPlatform> unresolved = Enum.GetValues<EnumStreamingPlatform>()
            .Where(platform => !resolved.ContainsKey(platform))
            .ToList();

        return new AdminResolveAlbumStreamingLinksResult(
            Resolved: resolved.Keys.OrderBy(platform => platform).ToList(),
            Unresolved: unresolved
        );
    }
}
