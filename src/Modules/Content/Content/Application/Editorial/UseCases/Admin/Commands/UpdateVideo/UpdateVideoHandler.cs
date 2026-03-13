using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo;

/// <summary>
/// Handles the <see cref="UpdateVideoCommand" /> to update a video's editable metadata fields.
/// Only permitted when the video is in <c>Draft</c> or <c>Rejected</c> status.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class UpdateVideoHandler(IVideoRepository videoRepository, IContentUnitOfWork unitOfWork, IMapper mapper)
    : ICommandHandler<UpdateVideoCommand, UpdateVideoResult>
{
    /// <inheritdoc />
    public async Task<UpdateVideoResult> Handle(UpdateVideoCommand command, CancellationToken cancellationToken)
    {
        Guid id = Guid.Parse(command.Id);

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        if (video.Status != EnumContentStatus.Draft && video.Status != EnumContentStatus.Rejected)
        {
            throw VideoErrors.InvalidStatusTransition(from: video.Status.ToString(), to: "Draft/Rejected (editable)");
        }

        if (video.Slug != command.Slug)
        {
            VideoEntity? existing = await videoRepository.GetBySlugAsync(
                slug: command.Slug,
                cancellationToken: cancellationToken
            );

            if (existing is not null && existing.Id != id)
            {
                throw VideoErrors.SlugAlreadyExists(slug: command.Slug);
            }
        }

        video.UpdateDetails(title: command.Title, slug: command.Slug, description: command.Description);

        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        VideoEntity updated = await videoRepository.GetByIdOrThrowAsync(
            id: video.Id,
            cancellationToken: cancellationToken
        );

        var dto = updated.ToVideoDetailDto(mapper);
        return new UpdateVideoResult(Video: dto);
    }
}
