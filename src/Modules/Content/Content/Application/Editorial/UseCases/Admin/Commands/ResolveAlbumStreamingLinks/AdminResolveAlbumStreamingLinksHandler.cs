using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Application.Shared.Services;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks;

/// <summary>
/// Handles the <see cref="AdminResolveAlbumStreamingLinksCommand" />: one provider call, then
/// an upsert per resolved platform in a single commit. Resolution never deletes — a platform
/// the provider had no link for leaves any hand-curated row untouched, so an outage cannot
/// strip curation.
/// </summary>
/// <param name="albumRepository">Repository for album data access operations.</param>
/// <param name="streamingLinkRepository">Repository for streaming link data access operations.</param>
/// <param name="resolutionService">External provider resolving one URL into all platforms.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminResolveAlbumStreamingLinksHandler(
    IAlbumRepository albumRepository,
    IStreamingLinkRepository streamingLinkRepository,
    IStreamingLinkResolutionService resolutionService,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<AdminResolveAlbumStreamingLinksCommand, AdminResolveAlbumStreamingLinksResult>
{
    /// <inheritdoc />
    public async Task<AdminResolveAlbumStreamingLinksResult> Handle(
        AdminResolveAlbumStreamingLinksCommand command,
        CancellationToken cancellationToken
    )
    {
        await albumRepository.GetByIdOrThrowAsync(id: command.AlbumId, cancellationToken: cancellationToken);

        // A provider failure surfaces as StreamingLinkResolutionException, mapped by the global
        // pipeline; the resolution service stays i18n-free.
        IReadOnlyDictionary<EnumStreamingPlatform, string> resolved = await resolutionService.ResolveAsync(
            sourceUrl: command.SourceUrl,
            cancellationToken: cancellationToken
        );

        if (resolved.Count == 0)
        {
            throw i18n.StreamingLink.NothingResolved();
        }

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
                streamingLinkRepository.Update(streamingLink: existing);
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
