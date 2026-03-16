using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveVideo;

/// <summary>
/// Handles the <see cref="AdminArchiveVideoCommand" /> to archive a video.
/// Note: Archiving is reversible — Cloudinary thumbnail assets are not deleted.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminArchiveVideoHandler(IVideoRepository videoRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<AdminArchiveVideoCommand, AdminArchiveVideoResult>
{
    /// <inheritdoc />
    public async Task<AdminArchiveVideoResult> Handle(
        AdminArchiveVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        bool archived = video.Archive();

        if (!archived)
        {
            throw VideoErrors.AlreadyArchived();
        }

        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminArchiveVideoResult(IsSuccess: true);
    }
}
