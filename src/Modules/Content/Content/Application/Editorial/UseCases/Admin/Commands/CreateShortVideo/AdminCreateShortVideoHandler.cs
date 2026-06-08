using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo;

/// <summary>
/// Handles the <see cref="AdminCreateShortVideoCommand" /> to upload and create a new short video clip.
/// The video file is tracked via <see cref="FileEntity" /> in the Core module.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="fileRepository">Repository for centralized file entity management.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminCreateShortVideoHandler(
    IShortVideoRepository shortVideoRepository,
    IFileRepository fileRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper,
    ContentI18n i18n
) : ICommandHandler<AdminCreateShortVideoCommand, AdminCreateShortVideoResult>
{
    /// <inheritdoc />
    public async Task<AdminCreateShortVideoResult> Handle(
        AdminCreateShortVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        ShortVideoEntity? existing = await shortVideoRepository.GetBySlugAsync(
            slug: command.Slug,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            throw i18n.ShortVideo.SlugAlreadyExists(slug: command.Slug);
        }

        string publicId = Guid.NewGuid().ToString();

        FileEntity videoFile = await fileRepository.UploadAndStoreVideoFileAsync(
            file: command.VideoFile,
            publicId: publicId,
            folder: "content/short-videos",
            originalFileName: command.VideoFile.FileName,
            mimeType: command.VideoFile.ContentType,
            cancellationToken: cancellationToken
        );

        ShortVideoEntity shortVideo;

        if (command.VideoId.HasValue)
        {
            shortVideo = ShortVideoEntity.CreateTeaser(
                id: Guid.NewGuid(),
                title: command.Title,
                slug: command.Slug,
                videoFileId: videoFile.Id,
                videoId: command.VideoId.Value,
                authorId: command.AuthorId,
                errors: i18n.ShortVideo
            );
        }
        else
        {
            shortVideo = ShortVideoEntity.CreateStandalone(
                id: Guid.NewGuid(),
                title: command.Title,
                slug: command.Slug,
                videoFileId: videoFile.Id,
                authorId: command.AuthorId,
                errors: i18n.ShortVideo
            );
        }

        await shortVideoRepository.AddAsync(shortVideo: shortVideo, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        ShortVideoEntity created = await shortVideoRepository.GetByIdOrThrowAsync(
            id: shortVideo.Id,
            cancellationToken: cancellationToken
        );

        var dto = await created.ToShortVideoDtoAsync(mapper, fileRepository, cancellationToken);
        return new AdminCreateShortVideoResult(ShortVideo: dto);
    }
}
