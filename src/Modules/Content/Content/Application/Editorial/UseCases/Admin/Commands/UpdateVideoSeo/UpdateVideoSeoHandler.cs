using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo;

/// <summary>
/// Handles the <see cref="UpdateVideoSeoCommand" /> to update a video's SEO metadata.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class UpdateVideoSeoHandler(IVideoRepository videoRepository, IContentUnitOfWork unitOfWork, IMapper mapper)
    : ICommandHandler<UpdateVideoSeoCommand, UpdateVideoSeoResult>
{
    /// <inheritdoc />
    public async Task<UpdateVideoSeoResult> Handle(UpdateVideoSeoCommand command, CancellationToken cancellationToken)
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

        var dto = updated.ToVideoDetailDto(mapper);
        return new UpdateVideoSeoResult(Video: dto);
    }
}
