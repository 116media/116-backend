using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishVideo;

/// <summary>
/// Handles the <see cref="PublishVideoCommand" /> to publish an approved video.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublishVideoHandler(IVideoRepository videoRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<PublishVideoCommand>
{
    /// <inheritdoc />
    public async Task Handle(PublishVideoCommand command, CancellationToken cancellationToken)
    {
        Guid id = Guid.Parse(command.Id);

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        if (video.Status != EnumContentStatus.Approved)
        {
            throw VideoErrors.InvalidStatusTransition(
                from: video.Status.ToString(),
                to: EnumContentStatus.Published.ToString()
            );
        }

        video.Publish();
        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }
}
