using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Services;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo;

/// <summary>
/// Handles the <see cref="CreateShortVideoCommand" /> to upload and create a new short video clip.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="cloudinaryService">Service for uploading and deleting media assets in cloud storage.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class CreateShortVideoHandler(
    IShortVideoRepository shortVideoRepository,
    ICloudinaryService cloudinaryService,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<CreateShortVideoCommand, CreateShortVideoResult>
{
    /// <inheritdoc />
    public async Task<CreateShortVideoResult> Handle(
        CreateShortVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        string storageKey = $"content/short-videos/{Guid.NewGuid()}";

        CloudinaryUploadResult uploadResult = await cloudinaryService.UploadImageAsync(
            file: command.VideoFile,
            publicId: storageKey,
            folder: "content/short-videos",
            cancellationToken: cancellationToken
        );

        ShortVideoEntity shortVideo;

        if (command.VideoId.HasValue)
        {
            shortVideo = ShortVideoEntity.CreateTeaser(
                id: Guid.NewGuid(),
                title: command.Title,
                videoUrl: uploadResult.SecureUrl,
                videoStorageKey: uploadResult.PublicId,
                videoId: command.VideoId.Value
            );
        }
        else
        {
            shortVideo = ShortVideoEntity.CreateStandalone(
                id: Guid.NewGuid(),
                title: command.Title,
                videoUrl: uploadResult.SecureUrl,
                videoStorageKey: uploadResult.PublicId
            );
        }

        await shortVideoRepository.AddAsync(shortVideo: shortVideo, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        ShortVideoEntity created = await shortVideoRepository.GetByIdOrThrowAsync(
            id: shortVideo.Id,
            cancellationToken: cancellationToken
        );

        var dto = created.ToShortVideoDto(mapper);
        return new CreateShortVideoResult(ShortVideo: dto);
    }
}
