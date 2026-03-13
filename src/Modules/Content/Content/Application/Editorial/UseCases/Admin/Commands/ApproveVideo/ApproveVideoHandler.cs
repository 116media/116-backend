using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveVideo;

/// <summary>
/// Handles the <see cref="ApproveVideoCommand" /> to approve a video pending editorial review.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class ApproveVideoHandler(IVideoRepository videoRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<ApproveVideoCommand>
{
    /// <inheritdoc />
    public async Task Handle(ApproveVideoCommand command, CancellationToken cancellationToken)
    {
        Guid id = Guid.Parse(command.Id);

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        if (video.Status != EnumContentStatus.PendingReview)
        {
            throw VideoErrors.InvalidStatusTransition(
                from: video.Status.ToString(),
                to: EnumContentStatus.Approved.ToString()
            );
        }

        video.Approve();
        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }
}
