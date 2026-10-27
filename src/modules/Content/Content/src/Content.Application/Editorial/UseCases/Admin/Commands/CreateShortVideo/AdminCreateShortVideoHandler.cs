using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Helpers;
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
public class AdminCreateShortVideoHandler(
    IShortVideoRepository shortVideoRepository,
    IContentUnitOfWork unitOfWork,
    IShortVideoDtoService shortVideoDtoService
) : ICommandHandler<AdminCreateShortVideoCommand, AdminCreateShortVideoResult>
{
    /// <inheritdoc />
    public async Task<AdminCreateShortVideoResult> Handle(
        AdminCreateShortVideoCommand command,
        CancellationToken cancellationToken = default
    )
    {
        string slug = await SlugHelper.ToAvailableSlugAsync(
            name: command.Title,
            isTakenAsync: async (candidate, ct) =>
                await shortVideoRepository.GetBySlugAsync(slug: candidate, cancellationToken: ct) is not null,
            unique: true,
            cancellationToken: cancellationToken
        );

        ShortVideoEntity shortVideo = command.VideoId is { } videoId
            ? ShortVideoEntity.CreateTeaser(
                id: Guid.NewGuid(),
                title: command.Title,
                slug: slug,
                videoId: videoId,
                authorId: command.AuthorId
            )
            : ShortVideoEntity.CreateStandalone(
                id: Guid.NewGuid(),
                title: command.Title,
                slug: slug,
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
