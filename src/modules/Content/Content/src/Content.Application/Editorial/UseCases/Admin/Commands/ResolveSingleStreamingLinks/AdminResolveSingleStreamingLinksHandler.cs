using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveSingleStreamingLinks.Contracts;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveSingleStreamingLinks;

/// <summary>
/// Handles the <see cref="AdminResolveSingleStreamingLinksCommand" /> to resolve and store a single's
/// streaming links.
/// </summary>
/// <param name="linkResolutionService">Service gating the single and resolving the links.</param>
/// <param name="streamingLinkRepository">Repository storing the resolved links.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminResolveSingleStreamingLinksHandler(
    IAdminSingleLinkResolutionService linkResolutionService,
    IStreamingLinkRepository streamingLinkRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminResolveSingleStreamingLinksCommand, AdminResolveSingleStreamingLinksResult>
{
    /// <inheritdoc />
    public async Task<AdminResolveSingleStreamingLinksResult> Handle(
        AdminResolveSingleStreamingLinksCommand command,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyDictionary<EnumStreamingPlatform, string> resolved = await linkResolutionService.ResolveAsync(
            lyricsId: command.LyricsId,
            sourceUrl: command.SourceUrl,
            cancellationToken: cancellationToken
        );

        foreach ((EnumStreamingPlatform platform, string url) in resolved)
        {
            StreamingLinkEntity? existing = await streamingLinkRepository.GetByLyricsAndPlatformAsync(
                lyricsId: command.LyricsId,
                platform: platform,
                cancellationToken: cancellationToken
            );

            if (existing is not null)
            {
                existing.UpdateUrl(url: url);
                continue;
            }

            StreamingLinkEntity streamingLink = StreamingLinkEntity.ForSingle(
                id: Guid.NewGuid(),
                lyricsId: command.LyricsId,
                platform: platform,
                url: url
            );
            await streamingLinkRepository.AddAsync(streamingLink: streamingLink, cancellationToken: cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        List<EnumStreamingPlatform> unresolved = Enum.GetValues<EnumStreamingPlatform>()
            .Where(platform => !resolved.ContainsKey(platform))
            .ToList();

        return new AdminResolveSingleStreamingLinksResult(
            Resolved: resolved.Keys.OrderBy(platform => platform).ToList(),
            Unresolved: unresolved
        );
    }
}
