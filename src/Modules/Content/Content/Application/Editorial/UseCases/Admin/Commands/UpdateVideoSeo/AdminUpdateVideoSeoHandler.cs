using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo;

/// <summary>
/// Handles the <see cref="AdminUpdateVideoSeoCommand" /> to update a video's SEO metadata.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="fileRepository">Repository for resolving file URLs.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminUpdateVideoSeoHandler(
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    IFileRepository fileRepository,
    IMapper mapper
) : ICommandHandler<AdminUpdateVideoSeoCommand, AdminUpdateVideoSeoResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateVideoSeoResult> Handle(
        AdminUpdateVideoSeoCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        video.UpdateSeo(metaTitle: command.MetaTitle, metaDescription: command.MetaDescription);

        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        VideoEntity updated = await videoRepository.GetByIdOrThrowAsync(
            id: video.Id,
            cancellationToken: cancellationToken
        );

        var dto = await updated.ToVideoDetailDtoAsync(mapper, fileRepository, cancellationToken);
        return new AdminUpdateVideoSeoResult(Video: dto);
    }
}
