using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnbookmarkShortVideo;

/// <summary>
/// Handles the <see cref="PublicUnbookmarkShortVideoCommand" /> to remove a bookmark from a short video.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicUnbookmarkShortVideoHandler(
    IShortVideoRepository shortVideoRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<PublicUnbookmarkShortVideoCommand, PublicUnbookmarkShortVideoResult>
{
    /// <inheritdoc />
    public async Task<PublicUnbookmarkShortVideoResult> Handle(
        PublicUnbookmarkShortVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        ShortVideoEntity shortVideo = await shortVideoRepository.GetByIdOrThrowAsync(
            id: command.ShortVideoId,
            cancellationToken: cancellationToken
        );

        bool hasBookmarked = await shortVideoRepository.HasBookmarkedAsync(
            userId: command.UserId,
            shortVideoId: command.ShortVideoId,
            cancellationToken: cancellationToken
        );

        if (!hasBookmarked)
        {
            throw ShortVideoInteractionErrors.BookmarkNotFound();
        }

        await shortVideoRepository.RemoveBookmarkAsync(
            userId: command.UserId,
            shortVideoId: command.ShortVideoId,
            cancellationToken: cancellationToken
        );

        shortVideo.DecrementBookmarkCount();
        shortVideoRepository.Update(shortVideo: shortVideo);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicUnbookmarkShortVideoResult(IsSuccess: true);
    }
}
