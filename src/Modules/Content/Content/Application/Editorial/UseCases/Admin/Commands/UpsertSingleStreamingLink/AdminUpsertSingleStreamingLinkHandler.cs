using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertSingleStreamingLink;

/// <summary>
/// Handles the <see cref="AdminUpsertSingleStreamingLinkCommand" /> to set or replace a
/// standalone single's curated streaming link for a single platform.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="streamingLinkRepository">Repository for streaming link data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminUpsertSingleStreamingLinkHandler(
    ILyricsRepository lyricsRepository,
    IStreamingLinkRepository streamingLinkRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<AdminUpsertSingleStreamingLinkCommand, AdminUpsertSingleStreamingLinkResult>
{
    /// <inheritdoc />
    public async Task<AdminUpsertSingleStreamingLinkResult> Handle(
        AdminUpsertSingleStreamingLinkCommand command,
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

        StreamingLinkEntity? existing = await streamingLinkRepository.GetByLyricsAndPlatformAsync(
            lyricsId: command.LyricsId,
            platform: command.Platform,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            existing.UpdateUrl(url: command.Url);
            streamingLinkRepository.Update(streamingLink: existing);
            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

            return new AdminUpsertSingleStreamingLinkResult(StreamingLinkId: existing.Id);
        }

        StreamingLinkEntity streamingLink = StreamingLinkEntity.ForSingle(
            id: Guid.NewGuid(),
            lyricsId: command.LyricsId,
            platform: command.Platform,
            url: command.Url
        );

        await streamingLinkRepository.AddAsync(streamingLink: streamingLink, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminUpsertSingleStreamingLinkResult(StreamingLinkId: streamingLink.Id);
    }
}
