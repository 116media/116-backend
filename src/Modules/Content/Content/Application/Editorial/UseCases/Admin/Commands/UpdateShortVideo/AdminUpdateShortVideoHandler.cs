using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo;

/// <summary>
/// Handles the <see cref="AdminUpdateShortVideoCommand" /> to update short video metadata.
/// The video file is replaced separately via the dedicated upload endpoint.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="fileRepository">Repository for resolving file URLs during mapping.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminUpdateShortVideoHandler(
    IShortVideoRepository shortVideoRepository,
    IFileRepository fileRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
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

        shortVideoRepository.Update(shortVideo);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        ShortVideoEntity updated = await shortVideoRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        var dto = await updated.ToShortVideoDtoAsync(mapper, fileRepository, cancellationToken);
        return new AdminUpdateShortVideoResult(ShortVideo: dto);
    }
}
