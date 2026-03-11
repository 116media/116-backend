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
/// Handles the <see cref="AdminUpdateVideoCommand" /> to update all editable video fields.
/// Allowed when the video status is <c>Draft</c>, <c>PendingPayment</c>, <c>PendingReview</c>,
/// or <c>Rejected</c>.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminUpdateVideoHandler(
    ICategoryRepository categoryRepository,
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminUpdateVideoCommand, AdminUpdateVideoResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateVideoResult> Handle(
        AdminUpdateVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        if (video.Status is EnumContentStatus.Approved or EnumContentStatus.Published or EnumContentStatus.Archived)
        {
            throw VideoErrors.InvalidStatusTransition(
                from: video.Status.ToString(),
                to: "Draft/PendingPayment/PendingReview/Rejected (editable)"
            );
        }

        await categoryRepository.GetByIdOrThrowAsync(id: command.CategoryId, cancellationToken: cancellationToken);

        if (command.Slug != video.Slug)
        {
            VideoEntity? slugConflict = await videoRepository.GetBySlugAsync(
                slug: command.Slug,
                cancellationToken: cancellationToken
            );

            if (slugConflict is not null && slugConflict.Id != video.Id)
            {
                throw VideoErrors.SlugAlreadyExists(slug: command.Slug);
            }
        }

        video.Update(
            categoryId: command.CategoryId,
            title: command.Title,
            slug: command.Slug,
            description: command.Description,
            customerId: command.CustomerId,
            orderItemId: command.OrderItemId,
            socialBoost: command.SocialBoost,
            isFeatured: command.IsFeatured,
            featuredUntil: command.FeaturedUntil,
            metaTitle: command.MetaTitle,
            metaDescription: command.MetaDescription
        );

        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        VideoEntity updated = await videoRepository.GetByIdOrThrowAsync(
            id: video.Id,
            cancellationToken: cancellationToken
        );

        var dto = updated.ToVideoDetailDto(mapper);
        return new AdminUpdateVideoResult(Video: dto);
    }
}
