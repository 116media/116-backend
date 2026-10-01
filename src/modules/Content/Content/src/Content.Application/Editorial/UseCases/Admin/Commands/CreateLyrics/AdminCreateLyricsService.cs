using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;

/// <summary>
/// Resolves and applies a lyrics creation for the admin create-lyrics use case.
/// </summary>
/// <param name="categoryRepository">Repository validating the category.</param>
/// <param name="lyricsRepository">Repository checking the slug and staging the lyrics.</param>
/// <param name="videoRepository">Repository validating the linked video.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminCreateLyricsService(
    ICategoryRepository categoryRepository,
    ILyricsRepository lyricsRepository,
    IVideoRepository videoRepository,
    ContentI18n i18n
) : IAdminCreateLyricsService
{
    /// <inheritdoc />
    public async Task<LyricsEntity> CreateAsync(AdminCreateLyricsCommand command, CancellationToken cancellationToken)
    {
        await categoryRepository.GetByIdOrThrowAsync(id: command.CategoryId, cancellationToken: cancellationToken);

        LyricsEntity? existing = await lyricsRepository.GetBySlugAsync(
            slug: command.Slug,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            throw i18n.Lyrics.SlugAlreadyExists(slug: command.Slug);
        }

        LyricsEntity lyrics = Build(command);

        if (command.VideoId is { } videoId)
        {
            await videoRepository.ExistsOrThrowAsync(videoId: videoId, cancellationToken: cancellationToken);
        }

        await lyricsRepository.AddAsync(lyrics: lyrics, cancellationToken: cancellationToken);

        return lyrics;
    }

    private static LyricsEntity Build(AdminCreateLyricsCommand command)
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
                slug: command.Slug,
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
                slug: command.Slug,
                authorId: command.AuthorId
            );
    }
}
