using _116.Mailer.Domain.Enums;

namespace _116.Mailer.Application.Shared.DTOs;

/// <summary>
/// Admin-facing projection of a newsletter subscriber.
/// </summary>
/// <param name="Id">The subscriber identifier.</param>
/// <param name="Email">The subscriber email address.</param>
/// <param name="Status">The double opt-in lifecycle state.</param>
/// <param name="ConfirmedAt">When the subscription was confirmed, if ever.</param>
/// <param name="UnsubscribedAt">When the subscriber opted out, if ever.</param>
/// <param name="CreatedAt">When the signup happened.</param>
public record NewsletterSubscriberDto(
    Guid Id,
    string Email,
    EnumNewsletterStatus Status,
    DateTime? ConfirmedAt,
    DateTime? UnsubscribedAt,
    DateTime? CreatedAt
);
