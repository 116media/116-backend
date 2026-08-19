using System.ComponentModel.DataAnnotations;
using _116.Mailer.Contracts.Application;
using _116.Shared.Domain;

namespace _116.Mailer.Domain.Entities;

/// <summary>
/// An in-app notification row for a platform user.
/// <para>
/// The row is self-contained: title and body are rendered and localized at
/// write time, so the feed never re-renders copy and old rows survive later
/// template changes. A null <see cref="ReadAt" /> means unread; the feed
/// orders by <c>CreatedAt</c> descending.
/// </para>
/// </summary>
public class NotificationEntity : Aggregate<Guid>
{
    /// <summary>
    /// The recipient platform user. A bare Identity-owned Guid without a
    /// foreign key, per the house cross-module pattern.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// The notification type that produced this row.
    /// </summary>
    public EnumNotificationType Type { get; private set; }

    /// <summary>
    /// The rendered, localized title.
    /// </summary>
    [MaxLength(length: 200)]
    public string Title { get; private set; } = null!;

    /// <summary>
    /// The rendered, localized body.
    /// </summary>
    [MaxLength(length: 500)]
    public string Body { get; private set; } = null!;

    /// <summary>
    /// The optional relative frontend path the notification links to
    /// (e.g. <c>/articles/slug</c>), never an absolute URL.
    /// </summary>
    [MaxLength(length: 300)]
    public string? LinkPath { get; private set; }

    /// <summary>
    /// When the user read the notification. Null means unread.
    /// </summary>
    public DateTime? ReadAt { get; private set; }

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private NotificationEntity() { }

    /// <summary>
    /// Creates an unread notification with its copy already rendered.
    /// </summary>
    /// <param name="id">The unique identifier for the notification.</param>
    /// <param name="userId">The recipient platform user.</param>
    /// <param name="type">The notification type, from the catalog.</param>
    /// <param name="title">The rendered, localized title.</param>
    /// <param name="body">The rendered, localized body.</param>
    /// <param name="linkPath">The optional relative frontend path.</param>
    /// <returns>A new unread <see cref="NotificationEntity" />.</returns>
    public static NotificationEntity Create(
        Guid id,
        Guid userId,
        EnumNotificationType type,
        string title,
        string body,
        string? linkPath
    )
    {
        return new NotificationEntity
        {
            Id = id,
            UserId = userId,
            Type = type,
            Title = title,
            Body = body,
            LinkPath = linkPath,
            ReadAt = null,
        };
    }

    /// <summary>
    /// Marks the notification read. Idempotent: marking an already read
    /// notification is a no-op that keeps the original read time.
    /// </summary>
    /// <param name="now">The current UTC time.</param>
    /// <returns>True when the notification transitioned to read; false when it already was.</returns>
    public bool MarkRead(DateTime now)
    {
        if (ReadAt is not null)
        {
            return false;
        }

        ReadAt = now;
        return true;
    }
}
