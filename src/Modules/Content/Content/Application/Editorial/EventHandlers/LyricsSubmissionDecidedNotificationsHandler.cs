using _116.Content.Application.Editorial.Messages;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Mailer.Contracts.Application.Messages;
using _116.Mailer.Contracts.Application.Services;
using _116.Mailer.Contracts.Domain.Enums;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Content.Application.Editorial.EventHandlers;

/// <summary>
/// Tells the submitter of a community lyrics submission how it was decided,
/// over email and the in-app feed. The email carries the moderator's review
/// note; an approval's in-app row links to the freshly published lyrics page.
/// Both channels are handled together because they share every lookup. Skips
/// entirely when the submitter or the submission cannot be resolved
/// post-commit; skips the email alone when the submitter has no email address
/// (OAuth accounts still get the in-app row).
/// </summary>
/// <param name="userLookupService">Lookup resolving the submitter's name and address by id.</param>
/// <param name="submissionRepository">Repository resolving the decided submission.</param>
/// <param name="lyricsRepository">Repository resolving the published lyrics page on approval.</param>
/// <param name="emailService">Outbox mailer sending the decision notice.</param>
/// <param name="notificationService">Writer for the in-app notification row.</param>
/// <param name="logger">Logger recording skipped deliveries.</param>
public class LyricsSubmissionDecidedNotificationsHandler(
    IUserLookupService userLookupService,
    ILyricsSubmissionRepository submissionRepository,
    ILyricsRepository lyricsRepository,
    IMessageDispatcher messageDispatcher,
    INotificationService notificationService,
    ILogger<LyricsSubmissionDecidedNotificationsHandler> logger
) : IDomainEventHandler<LyricsSubmissionDecidedEvent>
{
    /// <inheritdoc />
    public async Task Handle(LyricsSubmissionDecidedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        AuthorDto? submitter = await userLookupService.GetAuthorInfoByIdAsync(
            userId: domainEvent.SubmittedByUserId,
            ct: cancellationToken
        );

        if (submitter is null)
        {
            logger.LogDebug(
                "Submission decision notifications skipped: user {UserId} not found.",
                domainEvent.SubmittedByUserId
            );
            return;
        }

        LyricsSubmissionEntity? submission = await submissionRepository.GetByIdAsync(
            id: domainEvent.SubmissionId,
            cancellationToken: cancellationToken
        );

        if (submission is null)
        {
            logger.LogDebug(
                "Submission decision notifications skipped: submission {SubmissionId} not found.",
                domainEvent.SubmissionId
            );
            return;
        }

        string outcome = OutcomeWord(domainEvent.Outcome);

        if (submitter.Email is not null)
        {
            var message = new SubmissionDecidedMessage(
                Submitter: new MessageRecipient(
                    UserId: domainEvent.SubmittedByUserId,
                    Address: submitter.Email,
                    DisplayName: submitter.UserName,
                    Locale: submitter.PreferredLocale
                ),
                SongTitle: submission.SongTitle,
                Outcome: outcome,
                ReviewNote: domainEvent.ReviewNote ?? string.Empty
            );

            await messageDispatcher.DispatchAsync(message: message, cancellationToken: cancellationToken);
        }
        else
        {
            logger.LogDebug(
                "Submission decision email skipped: user {UserId} has no email address.",
                domainEvent.SubmittedByUserId
            );
        }

        var notificationTokens = new Dictionary<string, string>
        {
            ["songTitle"] = submission.SongTitle,
            ["outcome"] = outcome,
        };

        if (domainEvent.PublishedLyricsId.HasValue)
        {
            LyricsEntity? lyrics = await lyricsRepository.GetByIdAsync(
                id: domainEvent.PublishedLyricsId.Value,
                cancellationToken: cancellationToken
            );

            if (lyrics is not null)
            {
                notificationTokens["linkPath"] = $"/lyrics/{lyrics.Slug}";
            }
        }

        await notificationService.NotifyAsync(
            userId: domainEvent.SubmittedByUserId,
            type: EnumNotificationType.SubmissionDecided,
            tokens: notificationTokens,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Maps the decision outcome to the word substituted into the copy,
    /// following the same raw-token convention as the role-change notices.
    /// </summary>
    /// <param name="outcome">The submission's decided status.</param>
    /// <returns>The outcome wording used by the templates.</returns>
    private static string OutcomeWord(EnumSubmissionStatus outcome)
    {
        return outcome switch
        {
            EnumSubmissionStatus.Approved => "approved",
            EnumSubmissionStatus.Rejected => "rejected",
            _ => "returned for revision",
        };
    }
}
