using _116.Content.Application.Interactions.Constants;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RecordShortVideoView;

/// <summary>
/// Handles the <see cref="PublicRecordShortVideoViewCommand" />: stores a raw view event and
/// increments the displayed count only when the viewer's dedup key has no counted view inside
/// <see cref="ViewCountingConstants.DedupWindow" />.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicRecordShortVideoViewHandler(
    IShortVideoRepository shortVideoRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<PublicRecordShortVideoViewCommand, PublicRecordShortVideoViewResult>
{
    /// <summary>
    /// Dedup key used when the caller exposes no identity signal at all.
    /// </summary>
    private const string UnknownDedupKey = "unknown";

    /// <inheritdoc />
    public async Task<PublicRecordShortVideoViewResult> Handle(
        PublicRecordShortVideoViewCommand command,
        CancellationToken cancellationToken
    )
    {
        await shortVideoRepository.GetByIdOrThrowAsync(id: command.ShortVideoId, cancellationToken: cancellationToken);

        string dedupKey = ResolveDedupKey(command: command);

        DateTime windowStart = DateTime.UtcNow - ViewCountingConstants.DedupWindow;

        // "unknown" is a shared bucket, not an identity — deduplicating it would make
        // unrelated signal-less viewers suppress each other, so those always count.
        bool alreadyCounted =
            dedupKey != UnknownDedupKey
            && await shortVideoRepository.HasCountedViewSinceAsync(
                shortVideoId: command.ShortVideoId,
                dedupKey: dedupKey,
                since: windowStart,
                cancellationToken: cancellationToken
            );

        bool isCounted = !alreadyCounted;

        var viewEvent = ShortVideoViewEventEntity.Create(
            id: Guid.NewGuid(),
            shortVideoId: command.ShortVideoId,
            userId: command.UserId,
            dedupKey: dedupKey,
            ipAddress: command.IpAddress,
            userAgent: command.UserAgent,
            isCounted: isCounted
        );

        await shortVideoRepository.AddViewEventAsync(viewEvent: viewEvent, cancellationToken: cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicRecordShortVideoViewResult(IsSuccess: true, IsCounted: isCounted);
    }

    /// <summary>
    /// Resolves the identity surrogate the view is deduplicated against, preferring the
    /// strongest available signal: user id, then device id, then IP address.
    /// </summary>
    private static string ResolveDedupKey(PublicRecordShortVideoViewCommand command)
    {
        if (command.UserId is Guid userId)
        {
            return $"user:{userId}";
        }

        if (!string.IsNullOrWhiteSpace(command.DeviceId))
        {
            return $"device:{command.DeviceId}";
        }

        if (!string.IsNullOrWhiteSpace(command.IpAddress))
        {
            return $"ip:{command.IpAddress}";
        }

        return UnknownDedupKey;
    }
}
