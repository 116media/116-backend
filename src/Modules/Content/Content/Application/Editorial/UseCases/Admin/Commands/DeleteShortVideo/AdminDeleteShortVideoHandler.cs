using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteShortVideo;

/// <summary>
/// Handles the <see cref="AdminDeleteShortVideoCommand" /> to permanently delete a short video.
/// Captures the video and thumbnail file ids on the aggregate before removal so the post-commit
/// cleanup handler can soft-delete the file rows and purge the remote assets after the business
/// commit; a storage failure can no longer block or outlive the deletion.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminDeleteShortVideoHandler(IShortVideoRepository shortVideoRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<AdminDeleteShortVideoCommand, AdminDeleteShortVideoResult>
{
    /// <inheritdoc />
    public async Task<AdminDeleteShortVideoResult> Handle(
        AdminDeleteShortVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        ShortVideoEntity shortVideo = await shortVideoRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        shortVideo.MarkDeleted();
        shortVideoRepository.Remove(shortVideo: shortVideo);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminDeleteShortVideoResult(IsSuccess: true);
    }
}
