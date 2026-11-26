using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics.Contracts;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;

/// <summary>
/// Resolves and applies a lyrics update for the admin update-lyrics use case.
/// </summary>
/// <param name="categoryRepository">Repository validating the category.</param>
/// <param name="lyricsRepository">Repository loading the lyrics and checking the slug.</param>
/// <param name="videoRepository">Repository validating the linked video.</param>
public class AdminUpdateLyricsService(
    ICategoryRepository categoryRepository,
    ILyricsRepository lyricsRepository,
    IVideoRepository videoRepository
) : IAdminUpdateLyricsService
{
    /// <inheritdoc />
    public async Task<LyricsEntity> UpdateAsync(AdminUpdateLyricsCommand command, CancellationToken cancellationToken)
    {
        Guid id = Guid.Parse(command.Id);
        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);
        await categoryRepository.GetByIdOrThrowAsync(id: command.CategoryId, cancellationToken: cancellationToken);

        lyrics.Recategorize(categoryId: command.CategoryId);
        lyrics.Retitle(songTitle: command.SongTitle, artistName: command.ArtistName);
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
