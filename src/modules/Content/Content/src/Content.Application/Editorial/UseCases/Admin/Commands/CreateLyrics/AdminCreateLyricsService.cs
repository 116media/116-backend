using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics.Contracts;
using _116.Content.Application.Shared.Helpers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;

/// <summary>
/// Resolves and applies a lyrics creation for the admin create-lyrics use case.
/// </summary>
/// <param name="categoryRepository">Repository validating the category.</param>
/// <param name="lyricsRepository">Repository checking the slug and staging the lyrics.</param>
/// <param name="videoRepository">Repository validating the linked video.</param>
public class AdminCreateLyricsService(
    ICategoryRepository categoryRepository,
    ILyricsRepository lyricsRepository,
    IVideoRepository videoRepository
) : IAdminCreateLyricsService
{
    /// <inheritdoc />
    public async Task<LyricsEntity> CreateAsync(AdminCreateLyricsCommand command, CancellationToken cancellationToken)
    {
        await categoryRepository.GetByIdOrThrowAsync(id: command.CategoryId, cancellationToken: cancellationToken);

        string slug = await SlugHelper.ToAvailableSlugAsync(
            name: command.SongTitle,
            isTakenAsync: async (candidate, ct) =>
                await lyricsRepository.GetBySlugAsync(slug: candidate, cancellationToken: ct) is not null,
            cancellationToken: cancellationToken
        );

        LyricsEntity lyrics = Build(command, slug);

        if (command.VideoId is { } videoId)
        {
            await videoRepository.ExistsOrThrowAsync(videoId: videoId, cancellationToken: cancellationToken);
        }

        await lyricsRepository.AddAsync(lyrics: lyrics, cancellationToken: cancellationToken);

        return lyrics;
    }

    private static LyricsEntity Build(AdminCreateLyricsCommand command, string slug)
    {
        return command.CustomerId.HasValue
            ? LyricsEntity.CreatePaid(
                id: Guid.NewGuid(),
                customerId: command.CustomerId.Value,
                orderItemId: command.OrderItemId!.Value,
                categoryId: command.CategoryId,
                videoId: command.VideoId,
                songTitle: command.SongTitle,
                artistName: command.ArtistName,
                lyricsText: command.LyricsText,
                language: command.Language,
                slug: slug,
                authorId: command.AuthorId
            )
            : LyricsEntity.CreateFree(
                id: Guid.NewGuid(),
                categoryId: command.CategoryId,
                videoId: command.VideoId,
                songTitle: command.SongTitle,
                artistName: command.ArtistName,
                lyricsText: command.LyricsText,
                language: command.Language,
                slug: slug,
                authorId: command.AuthorId
            );
    }
}
