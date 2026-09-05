using _116.Mailer.Contracts.Domain;

namespace _116.Mailer.Application.Shared.DTOs;

/// <summary>
/// A user-facing notification feed row.
/// </summary>
/// <param name="Id">The notification identifier.</param>
/// <param name="Type">The notification type, from the catalog.</param>
/// <param name="Title">The rendered, localized title.</param>
/// <param name="Body">The rendered, localized body.</param>
/// <param name="LinkPath">The optional relative frontend path the notification links to.</param>
/// <param name="ReadAt">When the user read the notification; null means unread.</param>
/// <param name="CreatedAt">When the notification was written.</param>
public record NotificationDto(
    Guid Id,
    EnumNotificationType Type,
    string Title,
    string Body,
    string? LinkPath,
    DateTime? ReadAt,
    DateTime? CreatedAt
);
