using _116.BuildingBlocks.Application.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo;

/// <summary>
/// Resolves and applies a forced unpromotion for the admin force-unpromote use case.
/// </summary>
/// <param name="videoRepository">Repository resolving the video.</param>
/// <param name="currentActor">The acting admin.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
/// <param name="timeProvider">Clock stamping the unpromotion.</param>
public class AdminForceUnpromoteVideoService(
    IVideoRepository videoRepository,
    ICurrentActor currentActor,
    ContentI18n i18n,
    TimeProvider timeProvider
) : IAdminForceUnpromoteVideoService
{
    /// <inheritdoc />
    public async Task<VideoEntity> UnpromoteAsync(string slug, string reason, CancellationToken cancellationToken)
    {
        VideoEntity? video = await videoRepository.GetBySlugAsync(slug: slug, cancellationToken: cancellationToken);

        if (video is null)
        {
            throw i18n.Video.NotFound(Guid.Empty);
        }

        video.ForceUnpromote(unpromotedBy: currentActor.UserId!, reason: reason, now: timeProvider.GetUtcNow());

        return video;
    }
}
