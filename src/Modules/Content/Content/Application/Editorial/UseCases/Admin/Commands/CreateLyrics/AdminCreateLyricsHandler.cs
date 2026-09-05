using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Identity.Contracts.Application;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;

/// <summary>
/// Handles the <see cref="AdminCreateLyricsCommand" /> to create a new lyrics page.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">The Mapster mapper used for tags.</param>
/// <param name="userLookup">Service for resolving author profiles from the Identity module.</param>
/// <param name="fileRepository">Repository for resolving avatar file URLs.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminCreateLyricsHandler(
    ICategoryRepository categoryRepository,
    ILyricsRepository lyricsRepository,
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper,
    IUserLookupService userLookup,
    IFileRepository fileRepository,
    ContentI18n i18n
) : ICommandHandler<AdminCreateLyricsCommand, AdminCreateLyricsResult>
{
    /// <inheritdoc />
    public async Task<AdminCreateLyricsResult> Handle(
        AdminCreateLyricsCommand command,
        CancellationToken cancellationToken
    )
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

        LyricsEntity lyrics = CreateLyrics(command);

        if (command.VideoId.HasValue)
        {
            VideoEntity video = await videoRepository.GetByIdOrThrowAsync(
                id: command.VideoId.Value,
                cancellationToken: cancellationToken
            );
            video.MarkHasLyrics();
            videoRepository.Update(video: video);
        }

        await lyricsRepository.AddAsync(lyrics: lyrics, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        LyricsEntity created = await lyricsRepository.GetByIdOrThrowAsync(
            id: lyrics.Id,
            cancellationToken: cancellationToken
        );

        var dto = await created.ToLyricsDetailDtoAsync(mapper, userLookup, fileRepository, cancellationToken);
        return new AdminCreateLyricsResult(Lyrics: dto);
    }

    /// <summary>
    /// Creates a <see cref="LyricsEntity"/> based on the command payload.
    /// Produces a paid lyrics page when <see cref="AdminCreateLyricsCommand.CustomerId"/> is
    /// present, otherwise produces a free lyrics page.
    /// </summary>
    /// <param name="command">The command containing lyrics creation data.</param>
    /// <returns>A new <see cref="LyricsEntity"/> instance.</returns>
    private LyricsEntity CreateLyrics(AdminCreateLyricsCommand command)
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
