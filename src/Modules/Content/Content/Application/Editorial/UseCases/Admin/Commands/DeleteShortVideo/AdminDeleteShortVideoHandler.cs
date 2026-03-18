using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Services;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteShortVideo;

/// <summary>
/// Handles the <see cref="AdminDeleteShortVideoCommand" /> to permanently delete a short video
/// and its associated media assets from cloud storage.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="cloudinaryService">Service for uploading and deleting media assets in cloud storage.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminDeleteShortVideoHandler(
    IShortVideoRepository shortVideoRepository,
    ICloudinaryService cloudinaryService,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminDeleteShortVideoCommand, AdminDeleteShortVideoResult>
{
    /// <inheritdoc />
    public async Task<AdminDeleteShortVideoResult> Handle(
        AdminDeleteShortVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        ShortVideoEntity shortVideo = await shortVideoRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        if (shortVideo.ThumbnailStorageKey is not null)
        {
            await cloudinaryService.DeleteImageAsync(
                publicId: shortVideo.ThumbnailStorageKey,
                cancellationToken: cancellationToken
            );
        }

        await cloudinaryService.DeleteImageAsync(
            publicId: shortVideo.VideoStorageKey,
            cancellationToken: cancellationToken
        );

        shortVideoRepository.Remove(shortVideo: shortVideo);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminDeleteShortVideoResult(IsSuccess: true);
    }
}
