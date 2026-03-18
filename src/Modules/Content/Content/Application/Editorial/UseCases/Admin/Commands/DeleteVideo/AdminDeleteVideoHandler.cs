using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Services;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteVideo;

/// <summary>
/// Handles the <see cref="AdminDeleteVideoCommand" /> to permanently delete a draft or rejected video.
/// Deletes the Cloudinary thumbnail asset before removing the database record.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="cloudinaryService">Service for deleting Cloudinary image assets.</param>
public class AdminDeleteVideoHandler(
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    ICloudinaryService cloudinaryService
) : ICommandHandler<AdminDeleteVideoCommand, AdminDeleteVideoResult>
{
    /// <inheritdoc />
    public async Task<AdminDeleteVideoResult> Handle(
        AdminDeleteVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        if (video.Status != EnumContentStatus.Draft && video.Status != EnumContentStatus.Rejected)
        {
            throw VideoErrors.CannotDeletePublishedVideo();
        }

        if (video.ThumbnailStorageKey is not null)
        {
            await cloudinaryService.DeleteImageAsync(
                publicId: video.ThumbnailStorageKey,
                cancellationToken: cancellationToken
            );
        }

        videoRepository.Remove(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminDeleteVideoResult(IsSuccess: true);
    }
}
