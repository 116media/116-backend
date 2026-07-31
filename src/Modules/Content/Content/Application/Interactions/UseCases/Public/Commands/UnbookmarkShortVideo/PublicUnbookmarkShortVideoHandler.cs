using _116.Content.Application.Shared.Errors.Facade;
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
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicUnbookmarkShortVideoHandler(
    IShortVideoRepository shortVideoRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<PublicUnbookmarkShortVideoCommand, PublicUnbookmarkShortVideoResult>
{
    /// <inheritdoc />
    public async Task<PublicUnbookmarkShortVideoResult> Handle(
        PublicUnbookmarkShortVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        await shortVideoRepository.GetByIdOrThrowAsync(id: command.ShortVideoId, cancellationToken: cancellationToken);

        bool hasBookmarked = await shortVideoRepository.HasBookmarkedAsync(
            userId: command.UserId,
            shortVideoId: command.ShortVideoId,
            cancellationToken: cancellationToken
        );

        if (!hasBookmarked)
        {
            throw i18n.ShortVideoInteraction.BookmarkNotFound();
        }

        await shortVideoRepository.RemoveBookmarkAsync(
            userId: command.UserId,
            shortVideoId: command.ShortVideoId,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicUnbookmarkShortVideoResult(IsSuccess: true);
    }
}
