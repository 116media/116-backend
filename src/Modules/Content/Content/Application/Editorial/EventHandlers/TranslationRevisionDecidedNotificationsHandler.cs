using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Identity.Contracts.Application;
using _116.Mailer.Contracts.Application;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Content.Application.Editorial.EventHandlers;

/// <summary>
/// Tells the proposer of a translation correction revision how it was decided,
/// over email and the in-app feed, reusing the shared revision-decision copy.
/// Both channels are handled together because they share every lookup. Skips
/// entirely when the proposer, the translation, or its lyrics page cannot be
/// resolved post-commit; skips the email alone when the proposer has no email
/// address (OAuth accounts still get the in-app row).
/// </summary>
/// <param name="userLookupService">Lookup resolving the proposer's name and address by id.</param>
/// <param name="translationRepository">Repository resolving the corrected translation.</param>
/// <param name="lyricsRepository">Repository resolving the translated lyrics page.</param>
/// <param name="mailer">Outbox mailer sending the decision notice.</param>
/// <param name="notifier">Writer for the in-app notification row.</param>
/// <param name="logger">Logger recording skipped deliveries.</param>
public class TranslationRevisionDecidedNotificationsHandler(
    IUserLookupService userLookupService,
    ITranslationRepository translationRepository,
    ILyricsRepository lyricsRepository,
    IMailer mailer,
    INotifier notifier,
    ILogger<TranslationRevisionDecidedNotificationsHandler> logger
) : IDomainEventHandler<TranslationRevisionDecidedEvent>
{
    /// <inheritdoc />
    public async Task Handle(TranslationRevisionDecidedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        AuthorInfo? proposer = await userLookupService.GetAuthorInfoByIdAsync(
            userId: domainEvent.ProposedByUserId,
            ct: cancellationToken
        );

        if (proposer is null)
        {
            logger.LogDebug(
                "Translation revision decision notifications skipped: user {UserId} not found.",
                domainEvent.ProposedByUserId
            );
            return;
        }

        LyricsTranslationEntity? translation = await translationRepository.GetByIdAsync(
            id: domainEvent.TranslationId,
            cancellationToken: cancellationToken
        );

        LyricsEntity? lyrics = translation is null
            ? null
            : await lyricsRepository.GetByIdAsync(id: translation.LyricsId, cancellationToken: cancellationToken);

        if (translation is null || lyrics is null)
        {
            logger.LogDebug(
                "Translation revision decision notifications skipped: translation {TranslationId} not resolvable.",
                domainEvent.TranslationId
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
                "Translation revision decision email skipped: user {UserId} has no email address.",
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
