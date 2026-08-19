using _116.Mailer.Contracts.Application;
using _116.Mailer.Domain.Entities;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="NotificationEntity" />: creation state and the
/// idempotent mark-read transition.
/// </summary>
public class NotificationEntityTests
{
    private static NotificationEntity CreateNotification(string? linkPath = "/articles/eloko-oyo")
    {
        return NotificationEntity.Create(
            id: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            type: EnumNotificationType.CommentReply,
            title: "New reply to your comment",
            body: "Aline replied to your comment on Eloko Oyo.",
            linkPath: linkPath
        );
    }

    [Fact]
    public void Create_ShouldProduceAnUnreadSelfContainedRow()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        NotificationEntity notification = NotificationEntity.Create(
            id: id,
            userId: userId,
            type: EnumNotificationType.PasswordChanged,
            title: "Password changed",
            body: "Your password was changed.",
            linkPath: "/account/security"
        );

        notification.Id.Should().Be(id);
        notification.UserId.Should().Be(userId);
        notification.Type.Should().Be(EnumNotificationType.PasswordChanged);
        notification.Title.Should().Be("Password changed");
        notification.Body.Should().Be("Your password was changed.");
        notification.LinkPath.Should().Be("/account/security");
        notification.ReadAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithoutLinkPath_ShouldKeepTheLinkNull()
    {
        NotificationEntity notification = CreateNotification(linkPath: null);

        notification.LinkPath.Should().BeNull();
    }

    [Fact]
    public void MarkRead_OnAnUnreadNotification_ShouldSetTheReadTime()
    {
        NotificationEntity notification = CreateNotification();
        var now = new DateTime(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

        bool marked = notification.MarkRead(now);

        marked.Should().BeTrue();
        notification.ReadAt.Should().Be(now);
    }

    [Fact]
    public void MarkRead_OnAnAlreadyReadNotification_ShouldBeANoOpKeepingTheOriginalTime()
    {
        NotificationEntity notification = CreateNotification();
        var first = new DateTime(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);
        var second = first.AddHours(3);
        notification.MarkRead(first);

        bool marked = notification.MarkRead(second);

        marked.Should().BeFalse();
        notification.ReadAt.Should().Be(first);
    }
}
