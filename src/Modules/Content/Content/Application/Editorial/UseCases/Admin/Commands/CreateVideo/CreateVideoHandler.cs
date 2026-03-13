using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo;

/// <summary>
/// Handles the <see cref="CreateVideoCommand" /> to create a new video draft (step 1).
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class CreateVideoHandler(
    ICategoryRepository categoryRepository,
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<CreateVideoCommand, CreateVideoResult>
{
    /// <inheritdoc />
    public async Task<CreateVideoResult> Handle(CreateVideoCommand command, CancellationToken cancellationToken)
    {
        await categoryRepository.GetByIdOrThrowAsync(id: command.CategoryId, cancellationToken: cancellationToken);

        VideoEntity? existing = await videoRepository.GetBySlugAsync(
            slug: command.Slug,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            throw VideoErrors.SlugAlreadyExists(slug: command.Slug);
        }

        VideoEntity video;

        if (command.CustomerId.HasValue)
        {
            video = VideoEntity.CreatePaid(
                id: Guid.NewGuid(),
                customerId: command.CustomerId.Value,
                orderItemId: command.OrderItemId!.Value,
                categoryId: command.CategoryId,
                title: command.Title,
                slug: command.Slug,
                authorId: command.AuthorId,
                description: command.Description
            );
        }
        else
        {
            video = VideoEntity.CreateFree(
                id: Guid.NewGuid(),
                categoryId: command.CategoryId,
                title: command.Title,
                slug: command.Slug,
                authorId: command.AuthorId,
                description: command.Description
            );
        }

        if (command.ShootingScheduledAt.HasValue)
        {
            video.ScheduleShoot(command.ShootingScheduledAt.Value);
        }

        await videoRepository.AddAsync(video: video, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        VideoEntity created = await videoRepository.GetByIdOrThrowAsync(
            id: video.Id,
            cancellationToken: cancellationToken
        );

        var dto = created.ToVideoDetailDto(mapper);
        return new CreateVideoResult(Video: dto);
    }
}
