using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Identity.Contracts.Application;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Contracts.Domain;
using _116.Shared.Application.Localization;
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
/// <param name="mailer">Outbox mailer sending the decision notice.</param>
/// <param name="notifier">Writer for the in-app notification row.</param>
/// <param name="logger">Logger recording skipped deliveries.</param>
public class LyricsRevisionDecidedNotificationsHandler(
    IUserLookupService userLookupService,
    ILyricsRepository lyricsRepository,
    IMailer mailer,
    INotifier notifier,
    ILogger<LyricsRevisionDecidedNotificationsHandler> logger
) : IDomainEventHandler<LyricsRevisionDecidedEvent>
{
    /// <inheritdoc />
    public async Task Handle(LyricsRevisionDecidedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        AuthorInfo? proposer = await userLookupService.GetAuthorInfoByIdAsync(
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

        string culture = EmailCulture.Current();
        string decision = domainEvent.Accepted ? "accepted" : "rejected";

        if (proposer.Email is not null)
        {
            await mailer.EnqueueAsync(
                template: EnumEmailTemplate.RevisionDecided,
                to: new EmailRecipient(Address: proposer.Email, DisplayName: proposer.UserName),
                tokens: new Dictionary<string, string>
                {
                    ["userName"] = proposer.UserName,
                    ["songTitle"] = lyrics.SongTitle,
                    ["decision"] = decision,
                    ["lyricsUrl"] = ContentPublicLinks.Lyrics(lyrics.Slug),
                },
                culture: culture,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            logger.LogDebug(
                "Revision decision email skipped: user {UserId} has no email address.",
                domainEvent.ProposedByUserId
            );
        }

        await notifier.NotifyAsync(
            userId: domainEvent.ProposedByUserId,
            type: EnumNotificationType.RevisionDecided,
            tokens: new Dictionary<string, string>
            {
                ["songTitle"] = lyrics.SongTitle,
                ["decision"] = decision,
                ["linkPath"] = $"/lyrics/{lyrics.Slug}",
            },
            culture: culture,
            cancellationToken: cancellationToken
        );
    }
}
