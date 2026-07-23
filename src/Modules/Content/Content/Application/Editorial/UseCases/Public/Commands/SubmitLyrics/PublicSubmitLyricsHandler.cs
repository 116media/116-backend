using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics;

/// <summary>
/// Handles the <see cref="PublicSubmitLyricsCommand" /> to submit a new song, either directly
/// via the verified-artist fast path or into the community moderation queue.
/// </summary>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="submissionRepository">Repository for community lyrics submission data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicSubmitLyricsHandler(
    IArtistRepository artistRepository,
    ICategoryRepository categoryRepository,
    ILyricsRepository lyricsRepository,
    ILyricsSubmissionRepository submissionRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<PublicSubmitLyricsCommand, PublicSubmitLyricsResult>
{
    /// <inheritdoc />
    public async Task<PublicSubmitLyricsResult> Handle(
        PublicSubmitLyricsCommand command,
        CancellationToken cancellationToken
    )
    {
        // Identity-gated, never string-based: an owned artist profile is looked up strictly by
        // the submitter's own user id, so a mismatched ArtistName in the request can never
        // masquerade as this artist's own upload.
        ArtistEntity? ownedArtist = await artistRepository.GetByUserIdAsync(
            userId: command.UserId,
            cancellationToken: cancellationToken
        );

        return ownedArtist is not null
            ? await CreateForVerifiedArtistAsync(
                command: command,
                ownedArtist: ownedArtist,
                cancellationToken: cancellationToken
            )
            : await QueueForModerationAsync(command: command, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Creates the lyrics record directly for a submitter who owns a claimed artist profile,
    /// skipping the moderation queue entirely. Lands in <c>Draft</c> — no auto-publish — so it
    /// still goes through the normal spec-01 editorial workflow like any other lyrics record.
    /// </summary>
    private async Task<PublicSubmitLyricsResult> CreateForVerifiedArtistAsync(
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
            authorId: command.UserId,
            errors: i18n.Lyrics
        );
        lyrics.LinkArtist(artistId: ownedArtist.Id);

        await lyricsRepository.AddAsync(lyrics: lyrics, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicSubmitLyricsResult(WentToQueue: false, SubmissionId: null, LyricsId: lyrics.Id);
    }

    /// <summary>
    /// Queues the submission for moderation, since the submitter owns no claimed artist
    /// profile. Requires an artist name — the submission has nothing authoritative to fall
    /// back on.
    /// </summary>
    private async Task<PublicSubmitLyricsResult> QueueForModerationAsync(
        PublicSubmitLyricsCommand command,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(command.ArtistName))
        {
            throw i18n.Lyrics.ArtistNameRequired();
        }

        LyricsSubmissionEntity submission = LyricsSubmissionEntity.Submit(
            id: Guid.NewGuid(),
            songTitle: command.SongTitle,
            artistName: command.ArtistName,
            lyricsText: command.LyricsText,
            language: command.Language,
            userId: command.UserId
        );

        await submissionRepository.AddAsync(submission: submission, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicSubmitLyricsResult(WentToQueue: true, SubmissionId: submission.Id, LyricsId: null);
    }
}
