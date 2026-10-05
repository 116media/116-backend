using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo;

/// <summary>
/// Resolves and applies a video creation for the admin create-video use case.
/// </summary>
/// <param name="categoryRepository">Repository validating the category.</param>
/// <param name="videoRepository">Repository checking the slug and staging the video.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminCreateVideoService(
    ICategoryRepository categoryRepository,
    IVideoRepository videoRepository,
    ContentI18n i18n
) : IAdminCreateVideoService
{
    /// <inheritdoc />
    public async Task<VideoEntity> CreateAsync(AdminCreateVideoCommand command, CancellationToken cancellationToken)
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

        VideoEntity video = command.CustomerId.HasValue
            ? VideoEntity.CreatePaid(
                id: Guid.NewGuid(),
                customerId: command.CustomerId.Value,
                orderItemId: command.OrderItemId!.Value,
                categoryId: command.CategoryId,
                title: command.Title,
                slug: command.Slug,
                authorId: command.AuthorId,
                description: command.Description
            )
            : VideoEntity.CreateFree(
                id: Guid.NewGuid(),
                categoryId: command.CategoryId,
                title: command.Title,
                slug: command.Slug,
                authorId: command.AuthorId,
                description: command.Description
            );

        if (command.ShootingScheduledAt is { } shootingScheduledAt)
        {
            video.ScheduleShoot(shootingScheduledAt);
        }

        await videoRepository.AddAsync(video: video, cancellationToken: cancellationToken);

        return video;
    }
}
