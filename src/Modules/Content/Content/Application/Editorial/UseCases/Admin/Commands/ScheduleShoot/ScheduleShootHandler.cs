using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot;

/// <summary>
/// Handles the <see cref="ScheduleShootCommand" /> to schedule or update a video's shooting date.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class ScheduleShootHandler(IVideoRepository videoRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<ScheduleShootCommand>
{
    /// <inheritdoc />
    public async Task Handle(ScheduleShootCommand command, CancellationToken cancellationToken)
    {
        Guid videoId = Guid.Parse(command.VideoId);

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(
            id: videoId,
            cancellationToken: cancellationToken
        );

        video.ScheduleShoot(scheduledAt: command.ShootingScheduledAt);
        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }
}
