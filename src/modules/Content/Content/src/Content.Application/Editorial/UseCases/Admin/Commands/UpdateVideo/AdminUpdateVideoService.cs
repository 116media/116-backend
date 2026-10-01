using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo;

/// <summary>
/// Resolves and applies a video update for the admin update-video use case.
/// </summary>
/// <param name="categoryRepository">Repository validating the category.</param>
/// <param name="videoRepository">Repository loading the video and checking the slug.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminUpdateVideoService(
    ICategoryRepository categoryRepository,
    IVideoRepository videoRepository,
    ContentI18n i18n
) : IAdminUpdateVideoService
{
    /// <inheritdoc />
    public async Task<VideoEntity> UpdateAsync(AdminUpdateVideoCommand command, CancellationToken cancellationToken)
    {
        Guid id = Guid.Parse(command.Id);
        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);
        await categoryRepository.GetByIdOrThrowAsync(id: command.CategoryId, cancellationToken: cancellationToken);

        if (command.Slug != video.Slug)
        {
            VideoEntity? slugConflict = await videoRepository.GetBySlugAsync(
                slug: command.Slug,
                cancellationToken: cancellationToken
            );

            if (slugConflict is not null && slugConflict.Id != video.Id)
            {
                throw i18n.Video.SlugAlreadyExists(slug: command.Slug);
            }
        }

        video.Recategorize(categoryId: command.CategoryId);
        video.Retitle(title: command.Title, slug: command.Slug);
        video.ReviseDescription(description: command.Description);
        video.AssignCommission(
            customerId: command.CustomerId,
            orderItemId: command.OrderItemId,
            socialBoost: command.SocialBoost
        );
        video.ReviseSeo(metaTitle: command.MetaTitle, metaDescription: command.MetaDescription);

        return video;
    }
}
