using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Exceptions;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Application.Shared.Services;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveSingleStreamingLinks;

/// <summary>
/// Handles the <see cref="AdminResolveSingleStreamingLinksCommand" />: one provider call,
/// then an upsert per resolved platform in a single commit. Keeps the manual upsert's rule
/// that a song belonging to an album carries no single-level links — the album's links are
/// the release's links. Resolution never deletes an existing curated row.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="streamingLinkRepository">Repository for streaming link data access operations.</param>
/// <param name="resolutionService">External provider resolving one URL into all platforms.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminResolveSingleStreamingLinksHandler(
    ILyricsRepository lyricsRepository,
    IStreamingLinkRepository streamingLinkRepository,
    IStreamingLinkResolutionService resolutionService,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<AdminResolveSingleStreamingLinksCommand, AdminResolveSingleStreamingLinksResult>
{
    /// <inheritdoc />
    public async Task<AdminResolveSingleStreamingLinksResult> Handle(
        AdminResolveSingleStreamingLinksCommand command,
        CancellationToken cancellationToken
    )
    {
        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(
            id: command.LyricsId,
            cancellationToken: cancellationToken
        );

        if (lyrics.AlbumId.HasValue)
        {
            throw i18n.Lyrics.BelongsToAlbum();
        }

        IReadOnlyDictionary<EnumStreamingPlatform, string> resolved;

        try
        {
            resolved = await resolutionService.ResolveAsync(
                sourceUrl: command.SourceUrl,
                cancellationToken: cancellationToken
            );
        }
        catch (StreamingLinkResolutionException exception)
        {
            throw exception.IsRateLimited
                ? i18n.StreamingLink.ResolutionRateLimited()
                : i18n.StreamingLink.ResolutionFailed();
        }

        if (resolved.Count == 0)
        {
            throw i18n.StreamingLink.NothingResolved();
        }

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
                streamingLinkRepository.Update(streamingLink: existing);
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
