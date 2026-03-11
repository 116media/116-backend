using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;

/// <summary>
/// Handles the <see cref="AdminCreateLyricsCommand" /> to create a new lyrics page.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminCreateLyricsHandler(ILyricsRepository lyricsRepository, IContentUnitOfWork unitOfWork, IMapper mapper)
    : ICommandHandler<AdminCreateLyricsCommand, AdminCreateLyricsResult>
{
    /// <inheritdoc />
    public async Task<AdminCreateLyricsResult> Handle(
        AdminCreateLyricsCommand command,
        CancellationToken cancellationToken
    )
    {
        LyricsEntity? existing = await lyricsRepository.GetBySongTitleAndArtistAsync(
            songTitle: command.SongTitle,
            artistName: command.ArtistName,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            throw LyricsErrors.AlreadyExists(songTitle: command.SongTitle, artistName: command.ArtistName);
        }

        LyricsEntity lyrics;

        if (command.VideoId.HasValue)
        {
            lyrics = LyricsEntity.CreateForVideo(
                id: Guid.NewGuid(),
                videoId: command.VideoId.Value,
                songTitle: command.SongTitle,
                artistName: command.ArtistName,
                lyricsText: command.LyricsText,
                language: command.Language,
                authorId: command.AuthorId
            );
        }
        else if (command.ArticleId.HasValue)
        {
            lyrics = LyricsEntity.CreateForArticle(
                id: Guid.NewGuid(),
                articleId: command.ArticleId.Value,
                songTitle: command.SongTitle,
                artistName: command.ArtistName,
                lyricsText: command.LyricsText,
                language: command.Language,
                authorId: command.AuthorId
            );
        }
        else
        {
            lyrics = LyricsEntity.CreateStandalone(
                id: Guid.NewGuid(),
                songTitle: command.SongTitle,
                artistName: command.ArtistName,
                lyricsText: command.LyricsText,
                language: command.Language,
                authorId: command.AuthorId
            );
        }

        await lyricsRepository.AddAsync(lyrics: lyrics, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        LyricsEntity created = await lyricsRepository.GetByIdOrThrowAsync(
            id: lyrics.Id,
            cancellationToken: cancellationToken
        );

        var dto = created.ToLyricsDto(mapper);
        return new AdminCreateLyricsResult(Lyrics: dto);
    }
}
