using _116.Mailer.Application.Notifications.UseCases.Public.Queries.GetNotifications.V1;
using _116.Mailer.Application.Shared.DTOs;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;

namespace _116.Integration.Tests.Modules.Mailer.Application.Notifications.UseCases.Public.Queries.GetNotifications.V1;

/// <summary>
/// Integration tests for the PublicGetNotifications endpoint: feed ordering,
/// pagination, the unread-only filter, and ownership isolation.
/// </summary>
[Collection("Database")]
public class PublicGetNotificationsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static NotificationEntity CreateNotification(Guid userId, string title)
    {
        return NotificationEntity.Create(
            id: Guid.NewGuid(),
            userId: userId,
            type: EnumNotificationType.PasswordChanged,
            title: title,
            body: "Your password was changed.",
            linkPath: null
        );
    }

    /// <summary>
    /// Backdates a seeded notification's creation time; the audit interceptor
    /// only stamps added entities, so an update keeps the explicit value.
    /// </summary>
    private async Task BackdateAsync(Guid notificationId, DateTime createdAt)
    {
        await using MailerDbContext context = CreateDbContext<MailerDbContext>();
        NotificationEntity notification = await context.Notifications.SingleAsync(x => x.Id == notificationId);
        notification.CreatedAt = createdAt;
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Feed_ReturnsOwnNotificationsNewestFirst()
    {
        NotificationEntity older = CreateNotification(TestUser.VisitorId, "Older");
        NotificationEntity newer = CreateNotification(TestUser.VisitorId, "Newer");
        await SeedAsync<MailerDbContext>(ctx => ctx.Notifications.AddRange(older, newer));
        await BackdateAsync(older.Id, DateTime.UtcNow.AddDays(-2));
        await BackdateAsync(newer.Id, DateTime.UtcNow.AddDays(-1));
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ApiRoutes.Public.Notifications);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetNotificationsResponse body = await response.ReadAsAsync<PublicGetNotificationsResponse>();
        body.Notifications.Count.Should().Be(2);
        List<NotificationDto> items = [.. body.Notifications.Items];
        items[0].Title.Should().Be("Newer");
        items[1].Title.Should().Be("Older");
    }

    [Fact]
    public async Task Feed_Pagination_ReturnsTheRequestedPageWithTheTotalCount()
    {
        NotificationEntity first = CreateNotification(TestUser.VisitorId, "First");
        NotificationEntity second = CreateNotification(TestUser.VisitorId, "Second");
        NotificationEntity third = CreateNotification(TestUser.VisitorId, "Third");
        await SeedAsync<MailerDbContext>(ctx => ctx.Notifications.AddRange(first, second, third));
        await BackdateAsync(first.Id, DateTime.UtcNow.AddDays(-3));
        await BackdateAsync(second.Id, DateTime.UtcNow.AddDays(-2));
        await BackdateAsync(third.Id, DateTime.UtcNow.AddDays(-1));
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Notifications}?pageIndex=1&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetNotificationsResponse body = await response.ReadAsAsync<PublicGetNotificationsResponse>();
        body.Notifications.Count.Should().Be(3);
        body.Notifications.PageIndex.Should().Be(1);
        body.Notifications.PageSize.Should().Be(2);
        body.Notifications.Items.Should().ContainSingle().Which.Title.Should().Be("First");
    }

    [Fact]
    public async Task Feed_UnreadOnly_FiltersOutReadRows()
    {
        NotificationEntity read = CreateNotification(TestUser.VisitorId, "Read");
        read.MarkRead(DateTime.UtcNow);
        NotificationEntity unread = CreateNotification(TestUser.VisitorId, "Unread");
        await SeedAsync<MailerDbContext>(ctx => ctx.Notifications.AddRange(read, unread));
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Notifications}?unreadOnly=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetNotificationsResponse body = await response.ReadAsAsync<PublicGetNotificationsResponse>();
        body.Notifications.Count.Should().Be(1);
        body.Notifications.Items.Should().ContainSingle().Which.Title.Should().Be("Unread");
    }

    [Fact]
    public async Task Feed_NeverContainsAnotherUsersRows()
    {
        NotificationEntity own = CreateNotification(TestUser.VisitorId, "Mine");
        NotificationEntity foreign = CreateNotification(Guid.NewGuid(), "Not mine");
        await SeedAsync<MailerDbContext>(ctx => ctx.Notifications.AddRange(own, foreign));
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ApiRoutes.Public.Notifications);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetNotificationsResponse body = await response.ReadAsAsync<PublicGetNotificationsResponse>();
        body.Notifications.Count.Should().Be(1);
        body.Notifications.Items.Should().ContainSingle().Which.Id.Should().Be(own.Id);
    }

    [Fact]
    public async Task Feed_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync(ApiRoutes.Public.Notifications);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
