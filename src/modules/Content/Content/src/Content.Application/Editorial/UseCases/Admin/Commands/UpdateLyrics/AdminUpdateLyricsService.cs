using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;

/// <summary>
/// Resolves and applies a lyrics update for the admin update-lyrics use case.
/// </summary>
/// <param name="categoryRepository">Repository validating the category.</param>
/// <param name="lyricsRepository">Repository loading the lyrics and checking the slug.</param>
/// <param name="videoRepository">Repository validating the linked video.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminUpdateLyricsService(
    ICategoryRepository categoryRepository,
    ILyricsRepository lyricsRepository,
    IVideoRepository videoRepository,
    ContentI18n i18n
) : IAdminUpdateLyricsService
{
    /// <inheritdoc />
    public async Task<LyricsEntity> UpdateAsync(AdminUpdateLyricsCommand command, CancellationToken cancellationToken)
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

        if (command.VideoId is { } videoId)
        {
            await videoRepository.ExistsOrThrowAsync(videoId: videoId, cancellationToken: cancellationToken);
        }

        return lyrics;
    }
}
