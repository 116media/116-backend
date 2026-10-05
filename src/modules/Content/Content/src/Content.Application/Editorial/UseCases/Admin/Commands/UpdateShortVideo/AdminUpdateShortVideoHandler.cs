using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo;

/// <summary>
/// Handles the <see cref="AdminUpdateShortVideoCommand" /> to retitle or relink a short.
/// </summary>
/// <param name="shortVideoRepository">Repository loading and reloading the short.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="shortVideoDtoService">Service assembling the short DTO.</param>
public class AdminUpdateShortVideoHandler(
    IShortVideoRepository shortVideoRepository,
    IContentUnitOfWork unitOfWork,
    IShortVideoDtoService shortVideoDtoService
) : ICommandHandler<AdminUpdateShortVideoCommand, AdminUpdateShortVideoResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateShortVideoResult> Handle(
        AdminUpdateShortVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);
        ShortVideoEntity shortVideo = await shortVideoRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        shortVideo.Update(title: command.Title, videoId: command.VideoId);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        ShortVideoEntity updated = await shortVideoRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );
        var dto = await shortVideoDtoService.CreateAsync(updated, cancellationToken);
        return new AdminUpdateShortVideoResult(ShortVideo: dto);
    }
}
