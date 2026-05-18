using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.BookmarkShortVideo;

/// <summary>
/// Handles the <see cref="PublicBookmarkShortVideoCommand" /> to bookmark a short video.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="shortVideoInteractionErrors">Short video interaction domain error factory.</param>
public class PublicBookmarkShortVideoHandler(
    IShortVideoRepository shortVideoRepository,
    IContentUnitOfWork unitOfWork,
    ShortVideoInteractionErrors shortVideoInteractionErrors
) : ICommandHandler<PublicBookmarkShortVideoCommand, PublicBookmarkShortVideoResult>
{
    /// <inheritdoc />
    public async Task<PublicBookmarkShortVideoResult> Handle(
        PublicBookmarkShortVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        ShortVideoEntity shortVideo = await shortVideoRepository.GetByIdOrThrowAsync(
            id: command.ShortVideoId,
            cancellationToken: cancellationToken
        );

        bool alreadyBookmarked = await shortVideoRepository.HasBookmarkedAsync(
            userId: command.UserId,
            shortVideoId: command.ShortVideoId,
            cancellationToken: cancellationToken
        );

        if (alreadyBookmarked)
        {
            throw shortVideoInteractionErrors.AlreadyBookmarked();
        }

        var bookmark = ShortVideoBookmarkEntity.Create(
            id: Guid.NewGuid(),
            userId: command.UserId,
            shortVideoId: command.ShortVideoId
        );

        await shortVideoRepository.AddBookmarkAsync(bookmark: bookmark, cancellationToken: cancellationToken);

        shortVideo.IncrementBookmarkCount();
        shortVideoRepository.Update(shortVideo: shortVideo);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicBookmarkShortVideoResult(IsSuccess: true);
    }
}
