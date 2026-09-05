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
/// Tells the newly verified owner of an artist profile that the claim went
/// through, over email and the in-app feed. Both channels are handled together
/// because they share every lookup. Skips entirely when the owner or the
/// artist profile cannot be resolved post-commit; skips the email alone when
/// the owner has no email address (OAuth accounts still get the in-app row).
/// </summary>
/// <param name="userLookupService">Lookup resolving the owner's name and address by id.</param>
/// <param name="artistRepository">Repository resolving the verified artist profile.</param>
/// <param name="mailer">Outbox mailer sending the verification notice.</param>
/// <param name="notifier">Writer for the in-app notification row.</param>
/// <param name="logger">Logger recording skipped deliveries.</param>
public class ArtistOwnershipVerifiedNotificationsHandler(
    IUserLookupService userLookupService,
    IArtistRepository artistRepository,
    IMailer mailer,
    INotifier notifier,
    ILogger<ArtistOwnershipVerifiedNotificationsHandler> logger
) : IDomainEventHandler<ArtistOwnershipVerifiedEvent>
{
    /// <inheritdoc />
    public async Task Handle(ArtistOwnershipVerifiedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        AuthorInfo? owner = await userLookupService.GetAuthorInfoByIdAsync(
            userId: domainEvent.UserId,
            ct: cancellationToken
        );

        if (owner is null)
        {
            logger.LogDebug("Artist verification notifications skipped: user {UserId} not found.", domainEvent.UserId);
            return;
        }

        ArtistEntity? artist = await artistRepository.GetByIdAsync(
            id: domainEvent.ArtistId,
            cancellationToken: cancellationToken
        );

        if (artist is null)
        {
            logger.LogDebug(
                "Artist verification notifications skipped: artist {ArtistId} not found.",
                domainEvent.ArtistId
            );
            return;
        }

        string culture = EmailCulture.Current();

        if (owner.Email is not null)
        {
            await mailer.EnqueueAsync(
                template: EnumEmailTemplate.ArtistVerified,
                to: new EmailRecipient(Address: owner.Email, DisplayName: owner.UserName),
                tokens: new Dictionary<string, string>
                {
                    ["userName"] = owner.UserName,
                    ["artistName"] = artist.Name,
                    ["artistUrl"] = ContentPublicLinks.Artist(artist.Slug),
                },
                culture: culture,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            logger.LogDebug(
                "Artist verification email skipped: user {UserId} has no email address.",
                domainEvent.UserId
            );
        }

        await notifier.NotifyAsync(
            userId: domainEvent.UserId,
            type: EnumNotificationType.ArtistVerified,
            tokens: new Dictionary<string, string>
            {
                ["artistName"] = artist.Name,
                ["linkPath"] = $"/artists/{artist.Slug}",
            },
            culture: culture,
            cancellationToken: cancellationToken
        );
    }
}
