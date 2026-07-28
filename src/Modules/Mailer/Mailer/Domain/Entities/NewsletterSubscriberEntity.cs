using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using _116.Mailer.Domain.Constants;
using _116.Mailer.Domain.Enums;
using _116.Shared.Domain;
using Microsoft.AspNetCore.WebUtilities;

namespace _116.Mailer.Domain.Entities;

/// <summary>
/// A newsletter subscriber moving through the double opt-in lifecycle.
/// <para>
/// Only <see cref="EnumNewsletterStatus.Subscribed" /> rows ever receive
/// newsletter content; pending rows only ever receive the single confirmation
/// email. Confirm and unsubscribe are idempotent no-ops on re-click, because
/// email links get clicked more than once.
/// </para>
/// </summary>
public class NewsletterSubscriberEntity : Aggregate<Guid>
{
    /// <summary>
    /// The subscriber email address, lowercased before storage and unique.
    /// </summary>
    [MaxLength(length: 320)]
    public string Email { get; private set; } = null!;

    /// <summary>
    /// The double opt-in lifecycle state.
    /// </summary>
    public EnumNewsletterStatus Status { get; private set; }

    /// <summary>
    /// The url-safe token the confirmation link carries. Rotated when a
    /// confirmation is re-issued.
    /// </summary>
    [MaxLength(length: 64)]
    public string ConfirmationToken { get; private set; } = null!;

    /// <summary>
    /// The url-safe token the one-click unsubscribe link carries. Independent
    /// from the confirmation token and stable for the row's lifetime.
    /// </summary>
    [MaxLength(length: 64)]
    public string UnsubscribeToken { get; private set; } = null!;

    /// <summary>
    /// When the subscription was confirmed.
    /// </summary>
    public DateTime? ConfirmedAt { get; private set; }

    /// <summary>
    /// When the subscriber opted out.
    /// </summary>
    public DateTime? UnsubscribedAt { get; private set; }

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private NewsletterSubscriberEntity() { }

    /// <summary>
    /// Creates a pending subscription for an email address, generating both
    /// tokens.
    /// </summary>
    /// <param name="id">The unique identifier for the subscriber.</param>
    /// <param name="email">The subscriber email address; stored lowercased.</param>
    /// <returns>A new pending <see cref="NewsletterSubscriberEntity" />.</returns>
    public static NewsletterSubscriberEntity Subscribe(Guid id, string email)
    {
        return new NewsletterSubscriberEntity
        {
            Id = id,
            Email = email.Trim().ToLowerInvariant(),
            Status = EnumNewsletterStatus.PendingConfirmation,
            ConfirmationToken = GenerateToken(),
            UnsubscribeToken = GenerateToken(),
        };
    }

    /// <summary>
    /// Confirms the subscription. A no-op unless the row is pending, so
    /// re-clicking the link and confirming an unsubscribed row change nothing.
    /// </summary>
    /// <param name="now">The current UTC time.</param>
    /// <returns><c>true</c> when the state changed to subscribed.</returns>
    public bool Confirm(DateTime now)
    {
        if (Status != EnumNewsletterStatus.PendingConfirmation)
        {
            return false;
        }

        Status = EnumNewsletterStatus.Subscribed;
        ConfirmedAt = now;
        return true;
    }

    /// <summary>
    /// Opts the subscriber out. Idempotent: unsubscribing an already
    /// unsubscribed row is a no-op.
    /// </summary>
    /// <param name="now">The current UTC time.</param>
    /// <returns><c>true</c> when the state changed to unsubscribed.</returns>
    public bool Unsubscribe(DateTime now)
    {
        if (Status == EnumNewsletterStatus.Unsubscribed)
        {
            return false;
        }

        Status = EnumNewsletterStatus.Unsubscribed;
        UnsubscribedAt = now;
        return true;
    }

    /// <summary>
    /// Returns an unsubscribed (or still-pending) row to the pending state
    /// with a fresh confirmation token, for re-subscription and confirmation
    /// re-sends. A no-op on an already subscribed row.
    /// </summary>
    public void ReissueConfirmation()
    {
        if (Status == EnumNewsletterStatus.Subscribed)
        {
            return;
        }

        Status = EnumNewsletterStatus.PendingConfirmation;
        ConfirmationToken = GenerateToken();
        ConfirmedAt = null;
        UnsubscribedAt = null;
    }

    /// <summary>
    /// Generates a url-safe random token for confirmation and unsubscribe links.
    /// </summary>
    private static string GenerateToken()
    {
        return WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(MailerConstants.NewsletterTokenBytes));
    }
}
