using _116.BuildingBlocks.Application.Exceptions;
using _116.Mailer.Application.Notifications.UseCases.Public.Commands.MarkNotificationRead.V1;
using _116.Mailer.Application.Shared.Errors.Messages;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;
using _116.Mailer.TestData.Factories;

namespace _116.Mailer.Integration.Tests.Application.Notifications.UseCases.Public.Commands.MarkNotificationRead.V1;

/// <summary>
/// Integration tests for the PublicMarkNotificationRead endpoint: single-row
/// read transition, idempotency, and the existence-hiding not-found for rows
/// the caller does not own.
/// </summary>
[Collection("Database")]
public class PublicMarkNotificationReadEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<DateTime?> ReadAtOfAsync(Guid notificationId)
    {
        await using MailerDbContext context = CreateDbContext<MailerDbContext>();
        NotificationEntity notification = await context.Notifications.SingleAsync(x => x.Id == notificationId);
        return notification.ReadAt;
    }

    [Fact]
    public async Task MarkRead_AnUnreadOwnNotification_SetsTheReadTime()
    {
        NotificationEntity seeded = NotificationFactory.CreatePasswordChanged(TestUser.VisitorId);
        await SeedAsync<MailerDbContext>(ctx => ctx.Notifications.Add(seeded));
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsync($"{ApiRoutes.Public.Notifications}/{seeded.Id}/read", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicMarkNotificationReadResponse body = await response.ReadAsAsync<PublicMarkNotificationReadResponse>();
        body.IsRead.Should().BeTrue();
        (await ReadAtOfAsync(seeded.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task MarkRead_Twice_IsIdempotentAndKeepsTheOriginalReadTime()
    {
        NotificationEntity seeded = NotificationFactory.CreatePasswordChanged(TestUser.VisitorId);
        await SeedAsync<MailerDbContext>(ctx => ctx.Notifications.Add(seeded));
        Client.AuthenticateAsVisitor();

        await Client.PatchAsync($"{ApiRoutes.Public.Notifications}/{seeded.Id}/read", content: null);
        DateTime? firstReadAt = await ReadAtOfAsync(seeded.Id);
        var second = await Client.PatchAsync($"{ApiRoutes.Public.Notifications}/{seeded.Id}/read", content: null);

        second.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicMarkNotificationReadResponse body = await second.ReadAsAsync<PublicMarkNotificationReadResponse>();
        body.IsRead.Should().BeTrue();
        (await ReadAtOfAsync(seeded.Id)).Should().Be(firstReadAt);
    }

    [Fact]
    public async Task MarkRead_AnotherUsersRow_ReturnsNotFoundAndLeavesItUnread()
    {
        NotificationEntity foreign = NotificationFactory.CreatePasswordChanged(Guid.NewGuid());
        await SeedAsync<MailerDbContext>(ctx => ctx.Notifications.Add(foreign));
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsync($"{ApiRoutes.Public.Notifications}/{foreign.Id}/read", content: null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<NotificationErrorMessage>(m => m.NotificationNotFound())
        );
        (await ReadAtOfAsync(foreign.Id)).Should().BeNull();
    }

    [Fact]
    public async Task MarkRead_AnUnknownId_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsync(
            $"{ApiRoutes.Public.Notifications}/{Guid.NewGuid()}/read",
            content: null
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<NotificationErrorMessage>(m => m.NotificationNotFound())
        );
    }

    [Fact]
    public async Task MarkRead_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await Client.PatchAsync(
            $"{ApiRoutes.Public.Notifications}/{Guid.NewGuid()}/read",
            content: null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
