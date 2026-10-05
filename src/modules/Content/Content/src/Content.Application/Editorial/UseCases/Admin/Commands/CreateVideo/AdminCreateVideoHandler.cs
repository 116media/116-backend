using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo.Contracts;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo;

/// <summary>
/// Handles the <see cref="AdminCreateVideoCommand" /> to create a video.
/// </summary>
/// <param name="createVideoService">Service resolving and staging the video.</param>
/// <param name="videoRepository">Repository reloading the committed video.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="videoDtoService">Service assembling the video detail.</param>
public class AdminCreateVideoHandler(
    IAdminCreateVideoService createVideoService,
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    IVideoDtoService videoDtoService
) : ICommandHandler<AdminCreateVideoCommand, AdminCreateVideoResult>
{
    /// <inheritdoc />
    public async Task<AdminCreateVideoResult> Handle(
        AdminCreateVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        VideoEntity video = await createVideoService.CreateAsync(command, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        VideoEntity created = await videoRepository.GetByIdOrThrowAsync(
            id: video.Id,
            cancellationToken: cancellationToken
        );
        var dto = await videoDtoService.CreateDetailAsync(created, cancellationToken);
        return new AdminCreateVideoResult(Video: dto);
    }
}
