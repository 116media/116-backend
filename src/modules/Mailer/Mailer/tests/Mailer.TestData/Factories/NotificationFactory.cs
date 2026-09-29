using _116.Mailer.Contracts.Domain.Enums;
using _116.Mailer.Domain.Entities;
using _116.Mailer.TestData.Builders.Entities;

namespace _116.Mailer.TestData.Factories;

/// <summary>
/// Named aliases for <see cref="NotificationBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class NotificationFactory
{
    /// <summary>
    /// Creates an unread password-changed notification for the given recipient.
    /// </summary>
    /// <param name="userId">The identity user UUID the notification belongs to.</param>
    /// <returns>An unread NotificationEntity.</returns>
    public static NotificationEntity CreatePasswordChanged(Guid userId) =>
        CreatePasswordChanged(userId, "Password changed");

    /// <summary>
    /// Creates an unread password-changed notification carrying a specific title.
    /// </summary>
    /// <param name="userId">The identity user UUID the notification belongs to.</param>
    /// <param name="title">The rendered notification title.</param>
    /// <returns>An unread NotificationEntity.</returns>
    public static NotificationEntity CreatePasswordChanged(Guid userId, string title) =>
        new NotificationBuilder()
            .WithUserId(userId)
            .WithType(EnumNotificationType.PasswordChanged)
            .WithContent(title, "Your password was changed.")
            .WithLinkPath(null)
            .Build();
}
