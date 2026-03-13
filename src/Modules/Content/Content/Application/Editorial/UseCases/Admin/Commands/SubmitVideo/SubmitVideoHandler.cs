using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitVideo;

/// <summary>
/// Handles the <see cref="SubmitVideoCommand" /> to submit a video for review or payment.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class SubmitVideoHandler(IVideoRepository videoRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<SubmitVideoCommand>
{
    /// <inheritdoc />
    public async Task Handle(SubmitVideoCommand command, CancellationToken cancellationToken)
    {
        Guid id = Guid.Parse(command.Id);

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        if (video.CustomerId.HasValue)
        {
            video.Submit();
        }
        else
        {
            video.MarkPendingReview();
        }

        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }
}
