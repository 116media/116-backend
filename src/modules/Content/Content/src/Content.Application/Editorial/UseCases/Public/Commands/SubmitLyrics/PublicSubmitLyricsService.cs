using _116.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics;

/// <summary>
/// Resolves the verified-artist path of the public submit-lyrics use case.
/// </summary>
/// <param name="artistRepository">Repository resolving the submitter's own artist profile.</param>
/// <param name="categoryRepository">Repository resolving the default lyrics category.</param>
/// <param name="lyricsRepository">Repository checking the slug and staging the lyrics.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicSubmitLyricsService(
    IArtistRepository artistRepository,
    ICategoryRepository categoryRepository,
    ILyricsRepository lyricsRepository,
    ContentI18n i18n
) : IPublicSubmitLyricsService
{
    /// <inheritdoc />
    public Task<ArtistEntity?> FindOwnedArtistAsync(Guid userId, CancellationToken cancellationToken)
    {
        // Identity-gated, never string-based, so a mismatched ArtistName can never masquerade as this artist.
        return artistRepository.GetByUserIdAsync(userId: userId, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<LyricsEntity> CreateForArtistAsync(
        PublicSubmitLyricsCommand command,
        ArtistEntity ownedArtist,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(command.Slug))
        {
            throw i18n.Lyrics.SlugRequired();
        }

        CategoryEntity? category = await categoryRepository.GetDefaultLyricsCategoryAsync(
            cancellationToken: cancellationToken
        );

        if (category is null)
        {
            throw i18n.Category.DefaultLyricsCategoryNotConfigured();
        }

        LyricsEntity? existing = await lyricsRepository.GetBySlugAsync(
            slug: command.Slug,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            throw i18n.Lyrics.SlugAlreadyExists(slug: command.Slug);
        }

        LyricsEntity lyrics = LyricsEntity.CreateFree(
            id: Guid.NewGuid(),
            categoryId: category.Id,
            videoId: null,
            songTitle: command.SongTitle,
            artistName: ownedArtist.Name,
            lyricsText: command.LyricsText,
            language: command.Language,
            slug: command.Slug,
            authorId: command.UserId
        );
        lyrics.LinkArtist(artistId: ownedArtist.Id);

        await lyricsRepository.AddAsync(lyrics: lyrics, cancellationToken: cancellationToken);

        return lyrics;
    }
}
