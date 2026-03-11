using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeactivateShortVideo;

/// <summary>
/// Handles the <see cref="AdminDeactivateShortVideoCommand" /> to hide a short video from the public feed.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminDeactivateShortVideoHandler(IShortVideoRepository shortVideoRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<AdminDeactivateShortVideoCommand, AdminDeactivateShortVideoResult>
{
    /// <inheritdoc />
    public async Task<AdminDeactivateShortVideoResult> Handle(
        AdminDeactivateShortVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        ShortVideoEntity shortVideo = await shortVideoRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        bool deactivated = shortVideo.Deactivate();

        if (!deactivated)
        {
            throw ShortVideoErrors.AlreadyInactive();
        }

        shortVideoRepository.Update(shortVideo: shortVideo);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminDeactivateShortVideoResult(IsSuccess: true);
    }
}
