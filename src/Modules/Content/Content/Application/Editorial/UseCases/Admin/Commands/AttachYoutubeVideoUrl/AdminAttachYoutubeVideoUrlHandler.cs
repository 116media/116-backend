using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeVideoUrl;

/// <summary>
/// Handles the <see cref="AdminAttachYoutubeVideoUrlCommand" /> to attach a YouTube video URL.
/// The aggregate raises the attachment fact on commit and the post-commit thumbnail handler
/// downloads and attaches the YouTube thumbnail in its own scope, so a thumbnail outage no
/// longer fails the attach command.
/// </summary>
/// <param name="videoRepository">
/// Repository for video data access operations.
/// </param>
/// <param name="unitOfWork">
/// Unit of Work for managing database transactions.
/// </param>
/// <param name="fileRepository">
/// Repository for resolving file URLs during DTO mapping.
/// </param>
/// <param name="mapper">
/// Mapster mapper for entity-to-DTO transformations.
/// </param>
public class AdminAttachYoutubeVideoUrlHandler(
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    IFileRepository fileRepository,
    IMapper mapper
) : ICommandHandler<AdminAttachYoutubeVideoUrlCommand, AdminAttachYoutubeVideoUrlResult>
{
    /// <inheritdoc />
    public async Task<AdminAttachYoutubeVideoUrlResult> Handle(
        AdminAttachYoutubeVideoUrlCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid videoId = Guid.Parse(command.VideoId);

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(
            id: videoId,
            cancellationToken: cancellationToken
        );

        video.AttachYoutubeVideoUrl(youtubeVideoUrl: command.YoutubeVideoUrl);

        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        VideoEntity updated = await videoRepository.GetByIdOrThrowAsync(
            id: videoId,
            cancellationToken: cancellationToken
        );

        var dto = await updated.ToVideoDetailDtoAsync(mapper, fileRepository, cancellationToken);
        return new AdminAttachYoutubeVideoUrlResult(Video: dto);
    }
}
