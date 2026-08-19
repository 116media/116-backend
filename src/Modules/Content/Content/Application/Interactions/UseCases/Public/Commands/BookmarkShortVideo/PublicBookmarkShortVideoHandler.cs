using _116.Content.Application.Shared.Errors.Facade;
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
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicBookmarkShortVideoHandler(
    IShortVideoRepository shortVideoRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<PublicBookmarkShortVideoCommand, PublicBookmarkShortVideoResult>
{
    /// <inheritdoc />
    public async Task<PublicBookmarkShortVideoResult> Handle(
        PublicBookmarkShortVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        await shortVideoRepository.GetByIdOrThrowAsync(id: command.ShortVideoId, cancellationToken: cancellationToken);

        bool alreadyBookmarked = await shortVideoRepository.HasBookmarkedAsync(
            userId: command.UserId,
            shortVideoId: command.ShortVideoId,
            cancellationToken: cancellationToken
        );

        if (alreadyBookmarked)
        {
            throw i18n.ShortVideoInteraction.AlreadyBookmarked();
        }

        var bookmark = ShortVideoBookmarkEntity.Create(
            id: Guid.NewGuid(),
            userId: command.UserId,
            shortVideoId: command.ShortVideoId
        );

        await shortVideoRepository.AddBookmarkAsync(bookmark: bookmark, cancellationToken: cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicBookmarkShortVideoResult(IsSuccess: true);
    }
}
