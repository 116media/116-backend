using _116.Mailer.Application.Newsletter.UseCases.Public.Commands.SubscribeNewsletter.V1;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using _116.Mailer.Infrastructure.Persistence;

namespace _116.Integration.Tests.Modules.Mailer.Application.Newsletter.UseCases.Public.Commands.SubscribeNewsletter.V1;

/// <summary>
/// Integration tests for the PublicSubscribeNewsletter endpoint.
/// </summary>
[Collection("Database")]
public class PublicSubscribeNewsletterEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task Subscribe_NewAddress_ReturnsAcceptedAndPersistsPendingSubscriberWithConfirmEmail()
    {
        var response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Public.Newsletter}/subscriptions",
            new PublicSubscribeNewsletterRequest("fan@example.com")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        await using MailerDbContext ctx = CreateDbContext<MailerDbContext>();
        NewsletterSubscriberEntity? subscriber = await ctx.NewsletterSubscribers.FirstOrDefaultAsync(s =>
            s.Email == "fan@example.com"
        );
        subscriber.Should().NotBeNull();
        subscriber!.Status.Should().Be(EnumNewsletterStatus.PendingConfirmation);

        // The confirmation email is a self-contained outbox row carrying the token link.
        var outbox = await ctx.OutboxEmails.Where(o => o.RecipientAddress == "fan@example.com").ToListAsync();
        outbox.Should().ContainSingle(o => o.Template == "NewsletterConfirm");
        outbox[0].HtmlBody.Should().Contain(subscriber.ConfirmationToken);
    }

    /// <summary>
    /// A duplicate subscribe answers exactly like a fresh one — the response
    /// never reveals whether an address is already subscribed.
    /// </summary>
    [Fact]
    public async Task Subscribe_AlreadySubscribedAddress_IsNeutralAndSendsNothing()
    {
        await SeedAsync<MailerDbContext>(ctx =>
        {
            var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "loyal@example.com");
            subscriber.Confirm(DateTime.UtcNow);
            ctx.NewsletterSubscribers.Add(subscriber);
        });

        var response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Public.Newsletter}/subscriptions",
            new PublicSubscribeNewsletterRequest("loyal@example.com")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        await using MailerDbContext ctx = CreateDbContext<MailerDbContext>();
        (await ctx.OutboxEmails.CountAsync(o => o.RecipientAddress == "loyal@example.com")).Should().Be(0);
    }

    [Fact]
    public async Task Subscribe_UnsubscribedAddress_ReissuesConfirmationWithFreshToken()
    {
        NewsletterSubscriberEntity seeded = await SeedAsync<MailerDbContext, NewsletterSubscriberEntity>(ctx =>
        {
            var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "returning@example.com");
            subscriber.Confirm(DateTime.UtcNow);
            subscriber.Unsubscribe(DateTime.UtcNow);
            ctx.NewsletterSubscribers.Add(subscriber);
            return subscriber;
        });
        string oldToken = seeded.ConfirmationToken;

        var response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Public.Newsletter}/subscriptions",
            new PublicSubscribeNewsletterRequest("returning@example.com")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        await using MailerDbContext ctx = CreateDbContext<MailerDbContext>();
        NewsletterSubscriberEntity? subscriber = await ctx.NewsletterSubscribers.FirstAsync(s =>
            s.Email == "returning@example.com"
        );
        subscriber.Status.Should().Be(EnumNewsletterStatus.PendingConfirmation);
        subscriber.ConfirmationToken.Should().NotBe(oldToken);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public async Task Subscribe_InvalidEmail_ReturnsBadRequest(string email)
    {
        var response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Public.Newsletter}/subscriptions",
            new PublicSubscribeNewsletterRequest(email)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
