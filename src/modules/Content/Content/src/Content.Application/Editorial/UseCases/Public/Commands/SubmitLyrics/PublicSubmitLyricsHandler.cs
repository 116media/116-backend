using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics;

/// <summary>
/// Handles the <see cref="PublicSubmitLyricsCommand" />: a verified artist's upload publishes
/// directly, anyone else's is queued for moderation.
/// </summary>
/// <param name="submitService">Service resolving the owned artist and publishing under it.</param>
/// <param name="submissionRepository">Repository queueing the moderation submission.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicSubmitLyricsHandler(
    IPublicSubmitLyricsService submitService,
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
        ArtistEntity? ownedArtist = await submitService.FindOwnedArtistAsync(
            userId: command.UserId,
            cancellationToken: cancellationToken
        );

        if (ownedArtist is not null)
        {
            LyricsEntity lyrics = await submitService.CreateForArtistAsync(
                command: command,
                ownedArtist: ownedArtist,
                cancellationToken: cancellationToken
            );
            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

            return new PublicSubmitLyricsResult(WentToQueue: false, SubmissionId: null, LyricsId: lyrics.Id);
        }

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
