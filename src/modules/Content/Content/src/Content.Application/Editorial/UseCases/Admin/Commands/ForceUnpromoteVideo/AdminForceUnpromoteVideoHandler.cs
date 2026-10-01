using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo.Contracts;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo;

/// <summary>
/// Handles the <see cref="AdminForceUnpromoteVideoCommand" /> to end a video's promotion early.
/// </summary>
/// <param name="unpromoteService">Service resolving and unpromoting the video.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminForceUnpromoteVideoHandler(
    IAdminForceUnpromoteVideoService unpromoteService,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminForceUnpromoteVideoCommand, AdminForceUnpromoteVideoResult>
{
    /// <inheritdoc />
    public async Task<AdminForceUnpromoteVideoResult> Handle(
        AdminForceUnpromoteVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        VideoEntity video = await unpromoteService.UnpromoteAsync(
            slug: command.Slug,
            reason: command.Reason,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminForceUnpromoteVideoResult(VideoId: video.Id, UnpromotedAt: video.UnpromotedAt!.Value);
    }
}
