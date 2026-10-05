using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo.Contracts;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo;

/// <summary>
/// Handles the <see cref="AdminUpdateVideoCommand" /> to update a video.
/// </summary>
/// <param name="updateVideoService">Service resolving and applying the update.</param>
/// <param name="videoRepository">Repository reloading the committed video.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="videoDtoService">Service assembling the video detail.</param>
public class AdminUpdateVideoHandler(
    IAdminUpdateVideoService updateVideoService,
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    IVideoDtoService videoDtoService
) : ICommandHandler<AdminUpdateVideoCommand, AdminUpdateVideoResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateVideoResult> Handle(
        AdminUpdateVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        VideoEntity video = await updateVideoService.UpdateAsync(command, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        VideoEntity updated = await videoRepository.GetByIdOrThrowAsync(
            id: video.Id,
            cancellationToken: cancellationToken
        );
        var dto = await videoDtoService.CreateDetailAsync(updated, cancellationToken);
        return new AdminUpdateVideoResult(Video: dto);
    }
}
