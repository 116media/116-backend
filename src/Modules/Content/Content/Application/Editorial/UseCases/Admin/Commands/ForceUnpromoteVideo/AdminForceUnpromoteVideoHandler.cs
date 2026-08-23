using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Services;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo;

/// <summary>
/// Handles the <see cref="AdminForceUnpromoteVideoCommand" /> to force-unpromote a promoted video.
/// Fetches the video by slug, delegates the domain logic to <see cref="VideoEntity.ForceUnpromote" />,
/// and persists the audit fields for future pro-rata refund calculation.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="currentActor">Provides the identity of the authenticated user from JWT claims.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminForceUnpromoteVideoHandler(
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    ICurrentActor currentActor,
    ContentI18n i18n
) : ICommandHandler<AdminForceUnpromoteVideoCommand, AdminForceUnpromoteVideoResult>
{
    /// <inheritdoc />
    public async Task<AdminForceUnpromoteVideoResult> Handle(
        AdminForceUnpromoteVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        VideoEntity? video = await videoRepository.GetBySlugAsync(
            slug: command.Slug,
            cancellationToken: cancellationToken
        );

        if (video is null)
        {
            throw i18n.Video.NotFound(Guid.Empty);
        }

        video.ForceUnpromote(unpromotedBy: currentActor.UserId!, reason: command.Reason, errors: i18n.Video);

        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminForceUnpromoteVideoResult(VideoId: video.Id, UnpromotedAt: video.UnpromotedAt!.Value);
    }
}
