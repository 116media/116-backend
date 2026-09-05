using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo;

/// <summary>
/// Handles the <see cref="AdminCreateVideoCommand" /> to create a new video draft (step 1).
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="fileRepository">Repository for resolving file URLs.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminCreateVideoHandler(
    ICategoryRepository categoryRepository,
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    IFileRepository fileRepository,
    IMapper mapper,
    ContentI18n i18n
) : ICommandHandler<AdminCreateVideoCommand, AdminCreateVideoResult>
{
    /// <inheritdoc />
    public async Task<AdminCreateVideoResult> Handle(
        AdminCreateVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        await categoryRepository.GetByIdOrThrowAsync(id: command.CategoryId, cancellationToken: cancellationToken);

        VideoEntity? existing = await videoRepository.GetBySlugAsync(
            slug: command.Slug,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            throw i18n.Video.SlugAlreadyExists(slug: command.Slug);
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

        var dto = await created.ToVideoDetailDtoAsync(mapper, fileRepository, cancellationToken);
        return new AdminCreateVideoResult(Video: dto);
    }
}
