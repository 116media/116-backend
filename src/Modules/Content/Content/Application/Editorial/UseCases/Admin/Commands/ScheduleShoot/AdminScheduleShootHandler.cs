using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot;

/// <summary>
/// Handles the <see cref="AdminScheduleShootCommand" /> to schedule or update a video's shooting date.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminScheduleShootHandler(IVideoRepository videoRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<AdminScheduleShootCommand, AdminScheduleShootResult>
{
    /// <inheritdoc />
    public async Task<AdminScheduleShootResult> Handle(
        AdminScheduleShootCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid videoId = Guid.Parse(command.VideoId);

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(
            id: videoId,
            cancellationToken: cancellationToken
        );

        video.ScheduleShoot(scheduledAt: command.ShootingScheduledAt);
        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminScheduleShootResult(IsSuccess: true);
    }
}
