using _116.Mailer.Application.Notifications.UseCases.Public.Commands.MarkAllNotificationsRead;
using _116.Mailer.Application.Notifications.UseCases.Public.Commands.MarkNotificationRead;
using _116.Mailer.Application.Notifications.UseCases.Public.Queries.GetNotifications;
using _116.Mailer.Application.Notifications.UseCases.Public.Queries.GetUnreadNotificationCount;
using _116.Shared.Application.Metadata;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Application.Notifications.MetaFields;

/// <summary>
/// Tests that all Notifications public MetaField static fields are correctly initialized.
/// Accessing each static readonly field triggers its initializer, ensuring full coverage.
/// </summary>
public class NotificationsPublicMetaFieldTests
{
    #region Command MetaFields

    [Fact]
    public void PublicMarkAllNotificationsReadMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicMarkAllNotificationsReadMetaField.MarkAllNotificationsRead;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("PublicMarkAllNotificationsRead");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicMarkNotificationReadMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicMarkNotificationReadMetaField.MarkNotificationRead;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("PublicMarkNotificationRead");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Query MetaFields

    [Fact]
    public void PublicGetNotificationsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetNotificationsMetaField.GetNotifications;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("PublicGetNotifications");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetUnreadNotificationCountMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetUnreadNotificationCountMetaField.GetUnreadNotificationCount;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("PublicGetUnreadNotificationCount");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    #endregion
}
