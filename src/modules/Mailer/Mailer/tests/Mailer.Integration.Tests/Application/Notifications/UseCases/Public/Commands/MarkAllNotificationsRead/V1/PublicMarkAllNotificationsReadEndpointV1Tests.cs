using _116.Mailer.Application.Notifications.UseCases.Public.Commands.MarkAllNotificationsRead.V1;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;
using _116.Mailer.TestData.Factories;

namespace _116.Mailer.Integration.Tests.Application.Notifications.UseCases.Public.Commands.MarkAllNotificationsRead.V1;

/// <summary>
/// Integration tests for the PublicMarkAllNotificationsRead endpoint: bulk
/// read transition scoped to the caller, idempotent on repeat.
/// </summary>
[Collection("Database")]
public class PublicMarkAllNotificationsReadEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ReadAll_MarksEveryOwnUnreadRowAndLeavesOtherUsersUntouched()
    {
        NotificationEntity alreadyRead = NotificationFactory.CreatePasswordChanged(TestUser.VisitorId);
        alreadyRead.MarkRead(DateTime.UtcNow.AddDays(-1));
        NotificationEntity foreign = NotificationFactory.CreatePasswordChanged(Guid.NewGuid());
        await SeedAsync<MailerDbContext>(ctx =>
            ctx.Notifications.AddRange(
                NotificationFactory.CreatePasswordChanged(TestUser.VisitorId),
                NotificationFactory.CreatePasswordChanged(TestUser.VisitorId),
                alreadyRead,
                foreign
            )
        );
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsync($"{ApiRoutes.Public.Notifications}/read-all", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicMarkAllNotificationsReadResponse body =
            await response.ReadAsAsync<PublicMarkAllNotificationsReadResponse>();
        body.MarkedCount.Should().Be(2);

        await using MailerDbContext context = CreateDbContext<MailerDbContext>();
        int ownUnread = await context.Notifications.CountAsync(x => x.UserId == TestUser.VisitorId && x.ReadAt == null);
        ownUnread.Should().Be(0);
        NotificationEntity untouched = await context.Notifications.SingleAsync(x => x.Id == foreign.Id);
        untouched.ReadAt.Should().BeNull();
    }

    [Fact]
    public async Task ReadAll_SecondCall_FindsNothingUnreadAndMarksZero()
    {
        await SeedAsync<MailerDbContext>(ctx =>
            ctx.Notifications.Add(NotificationFactory.CreatePasswordChanged(TestUser.VisitorId))
        );
        Client.AuthenticateAsVisitor();

        await Client.PatchAsync($"{ApiRoutes.Public.Notifications}/read-all", content: null);
        var second = await Client.PatchAsync($"{ApiRoutes.Public.Notifications}/read-all", content: null);

        second.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicMarkAllNotificationsReadResponse body =
            await second.ReadAsAsync<PublicMarkAllNotificationsReadResponse>();
        body.MarkedCount.Should().Be(0);
    }

    [Fact]
    public async Task ReadAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await Client.PatchAsync($"{ApiRoutes.Public.Notifications}/read-all", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
