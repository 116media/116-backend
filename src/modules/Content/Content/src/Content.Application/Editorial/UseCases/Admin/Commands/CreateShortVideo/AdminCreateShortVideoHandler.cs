using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo;

/// <summary>
/// Handles the <see cref="AdminCreateShortVideoCommand" /> to create a short, standalone or as a teaser of a video.
/// </summary>
/// <param name="shortVideoRepository">Repository checking the slug, staging and reloading the short.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="shortVideoDtoService">Service assembling the short DTO.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminCreateShortVideoHandler(
    IShortVideoRepository shortVideoRepository,
    IContentUnitOfWork unitOfWork,
    IShortVideoDtoService shortVideoDtoService,
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

        ShortVideoEntity shortVideo = command.VideoId is { } videoId
            ? ShortVideoEntity.CreateTeaser(
                id: Guid.NewGuid(),
                title: command.Title,
                slug: command.Slug,
                videoId: videoId,
                authorId: command.AuthorId
            )
            : ShortVideoEntity.CreateStandalone(
                id: Guid.NewGuid(),
                title: command.Title,
                slug: command.Slug,
                authorId: command.AuthorId
            );

        await shortVideoRepository.AddAsync(shortVideo: shortVideo, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        ShortVideoEntity created = await shortVideoRepository.GetByIdOrThrowAsync(
            id: shortVideo.Id,
            cancellationToken: cancellationToken
        );
        var dto = await shortVideoDtoService.CreateAsync(created, cancellationToken);
        return new AdminCreateShortVideoResult(ShortVideo: dto);
    }
}
