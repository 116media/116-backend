using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ActivateShortVideo;

/// <summary>
/// Handles the <see cref="AdminActivateShortVideoCommand" /> to make a short video visible on the public feed.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminActivateShortVideoHandler(IShortVideoRepository shortVideoRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<AdminActivateShortVideoCommand, AdminActivateShortVideoResult>
{
    /// <inheritdoc />
    public async Task<AdminActivateShortVideoResult> Handle(
        AdminActivateShortVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        ShortVideoEntity shortVideo = await shortVideoRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        bool activated = shortVideo.Activate();

        if (!activated)
        {
            throw ShortVideoErrors.AlreadyActive();
        }

        shortVideoRepository.Update(shortVideo: shortVideo);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminActivateShortVideoResult(IsSuccess: true);
    }
}
