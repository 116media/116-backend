using _116.Mailer.Application.Notifications.UseCases.Public.Queries.GetUnreadNotificationCount.V1;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;
using _116.Mailer.TestData.Factories;

namespace _116.Mailer.Integration.Tests.Application.Notifications.UseCases.Public.Queries.GetUnreadNotificationCount.V1;

/// <summary>
/// Integration tests for the PublicGetUnreadNotificationCount endpoint: the
/// badge count covers own unread rows only.
/// </summary>
[Collection("Database")]
public class PublicGetUnreadNotificationCountEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UnreadCount_CountsOwnUnreadRowsOnly()
    {
        NotificationEntity read = NotificationFactory.CreatePasswordChanged(TestUser.VisitorId);
        read.MarkRead(DateTime.UtcNow);
        await SeedAsync<MailerDbContext>(ctx =>
            ctx.Notifications.AddRange(
                NotificationFactory.CreatePasswordChanged(TestUser.VisitorId),
                NotificationFactory.CreatePasswordChanged(TestUser.VisitorId),
                read,
                NotificationFactory.CreatePasswordChanged(Guid.NewGuid())
            )
        );
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Notifications}/unread-count");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetUnreadNotificationCountResponse body =
            await response.ReadAsAsync<PublicGetUnreadNotificationCountResponse>();
        body.Count.Should().Be(2);
    }

    [Fact]
    public async Task UnreadCount_WithNoNotifications_ReturnsZero()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Notifications}/unread-count");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetUnreadNotificationCountResponse body =
            await response.ReadAsAsync<PublicGetUnreadNotificationCountResponse>();
        body.Count.Should().Be(0);
    }

    [Fact]
    public async Task UnreadCount_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync($"{ApiRoutes.Public.Notifications}/unread-count");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
