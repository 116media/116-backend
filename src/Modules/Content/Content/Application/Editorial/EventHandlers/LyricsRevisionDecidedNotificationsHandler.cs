using _116.Content.Application.Editorial.OutboundEmails;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Mailer.Contracts.Application.OutboundEmails;
using _116.Mailer.Contracts.Application.Services;
using _116.Mailer.Contracts.Domain.Enums;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Content.Application.Editorial.EventHandlers;

/// <summary>
/// Tells the proposer of a lyrics-text correction revision how it was decided,
/// over email and the in-app feed. Both channels are handled together because
/// they share every lookup. Skips entirely when the proposer or the lyrics
/// page cannot be resolved post-commit; skips the email alone when the
/// proposer has no email address (OAuth accounts still get the in-app row).
/// </summary>
/// <param name="userLookupService">Lookup resolving the proposer's name and address by id.</param>
/// <param name="lyricsRepository">Repository resolving the corrected lyrics page.</param>
/// <param name="emailService">Outbox mailer sending the decision notice.</param>
/// <param name="notificationService">Writer for the in-app notification row.</param>
/// <param name="logger">Logger recording skipped deliveries.</param>
public class LyricsRevisionDecidedNotificationsHandler(
    IUserLookupService userLookupService,
    ILyricsRepository lyricsRepository,
    IEmailDispatcher messageDispatcher,
    INotificationService notificationService,
    ILogger<LyricsRevisionDecidedNotificationsHandler> logger
) : IDomainEventHandler<LyricsRevisionDecidedEvent>
{
    /// <inheritdoc />
    public async Task Handle(LyricsRevisionDecidedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        AuthorDto? proposer = await userLookupService.GetAuthorInfoByIdAsync(
            userId: domainEvent.ProposedByUserId,
            ct: cancellationToken
        );

        if (proposer is null)
        {
            logger.LogDebug(
                "Revision decision notifications skipped: user {UserId} not found.",
                domainEvent.ProposedByUserId
            );
            return;
        }

        LyricsEntity? lyrics = await lyricsRepository.GetByIdAsync(
            id: domainEvent.LyricsId,
            cancellationToken: cancellationToken
        );

        if (lyrics is null)
        {
            logger.LogDebug(
                "Revision decision notifications skipped: lyrics {LyricsId} not found.",
                domainEvent.LyricsId
            );
            return;
        }

        string decision = domainEvent.Accepted ? "accepted" : "rejected";

        if (proposer.Email is not null)
        {
            var message = new RevisionDecidedEmail(
                Proposer: new EmailRecipient(
                    UserId: domainEvent.ProposedByUserId,
                    Address: proposer.Email,
                    DisplayName: proposer.UserName,
                    Locale: proposer.PreferredLocale
                ),
                SongTitle: lyrics.SongTitle,
                Decision: decision,
                LyricsUrl: ContentPublicLinks.Lyrics(lyrics.Slug)
            );

            await messageDispatcher.DispatchAsync(message: message, cancellationToken: cancellationToken);
        }
        else
        {
            logger.LogDebug(
                "Revision decision email skipped: user {UserId} has no email address.",
                domainEvent.ProposedByUserId
            );
        }

        await notificationService.NotifyAsync(
            userId: domainEvent.ProposedByUserId,
            type: EnumNotificationType.RevisionDecided,
            tokens: new Dictionary<string, string>
            {
                ["songTitle"] = lyrics.SongTitle,
                ["decision"] = decision,
                ["linkPath"] = $"/lyrics/{lyrics.Slug}",
            },
            cancellationToken: cancellationToken
        );
    }
}
