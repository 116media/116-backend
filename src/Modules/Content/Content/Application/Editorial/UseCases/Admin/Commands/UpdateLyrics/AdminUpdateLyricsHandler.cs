using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.Services;
using _116.Identity.Contracts.Application.Services;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;

/// <summary>
/// Handles the <see cref="AdminUpdateLyricsCommand" /> to update the content
/// and metadata of an existing lyrics page.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">The Mapster mapper used for tags.</param>
/// <param name="userLookup">Service for resolving author profiles from the Identity module.</param>
/// <param name="fileStorage">Core's storage contract.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminUpdateLyricsHandler(
    ICategoryRepository categoryRepository,
    ILyricsRepository lyricsRepository,
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper,
    IUserLookupService userLookup,
    IFileStorageService fileStorage,
    ContentI18n i18n,
    IContentLookupFactory contentLookupFactory
) : ICommandHandler<AdminUpdateLyricsCommand, AdminUpdateLyricsResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateLyricsResult> Handle(
        AdminUpdateLyricsCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        await categoryRepository.GetByIdOrThrowAsync(id: command.CategoryId, cancellationToken: cancellationToken);

        if (command.Slug != lyrics.Slug)
        {
            LyricsEntity? slugConflict = await lyricsRepository.GetBySlugAsync(
                slug: command.Slug,
                cancellationToken: cancellationToken
            );

            if (slugConflict is not null && slugConflict.Id != lyrics.Id)
            {
                throw i18n.Lyrics.SlugAlreadyExists(slug: command.Slug);
            }
        }

        lyrics.Recategorize(categoryId: command.CategoryId);
        lyrics.Retitle(songTitle: command.SongTitle, artistName: command.ArtistName, slug: command.Slug);
        lyrics.ReviseText(lyricsText: command.LyricsText, language: command.Language);
        lyrics.Relink(videoId: command.VideoId);
        lyrics.AssignCommission(customerId: command.CustomerId, orderItemId: command.OrderItemId);

        if (command.VideoId.HasValue)
        {
            await videoRepository.ExistsOrThrowAsync(
                videoId: command.VideoId.Value,
                cancellationToken: cancellationToken
            );
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        LyricsEntity updated = await lyricsRepository.GetByIdOrThrowAsync(
            id: lyrics.Id,
            cancellationToken: cancellationToken
        );

        var dto = await updated.ToLyricsDetailDtoAsync(
            await contentLookupFactory.ResolveForLyricsAsync([updated], cancellationToken),
            mapper,
            userLookup,
            fileStorage,
            cancellationToken
        );
        return new AdminUpdateLyricsResult(Lyrics: dto);
    }
}
