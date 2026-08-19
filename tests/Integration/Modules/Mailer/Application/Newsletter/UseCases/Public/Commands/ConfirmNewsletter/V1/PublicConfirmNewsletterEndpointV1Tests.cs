using _116.Mailer.Application.Newsletter.UseCases.Public.Commands.ConfirmNewsletter.V1;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using _116.Mailer.Infrastructure.Persistence;

namespace _116.Integration.Tests.Modules.Mailer.Application.Newsletter.UseCases.Public.Commands.ConfirmNewsletter.V1;

/// <summary>
/// Integration tests for the PublicConfirmNewsletter endpoint.
/// </summary>
[Collection("Database")]
public class PublicConfirmNewsletterEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task Confirm_PendingToken_SubscribesAndSendsWelcomeWithUnsubscribeLink()
    {
        NewsletterSubscriberEntity seeded = await SeedAsync<MailerDbContext, NewsletterSubscriberEntity>(ctx =>
        {
            var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "pending@example.com");
            ctx.NewsletterSubscribers.Add(subscriber);
            return subscriber;
        });

        var response = await Client.GetAsync($"{ApiRoutes.Public.Newsletter}/confirm/{seeded.ConfirmationToken}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicConfirmNewsletterResponse body = await response.ReadAsAsync<PublicConfirmNewsletterResponse>();
        body.IsSubscribed.Should().BeTrue();

        await using MailerDbContext ctx = CreateDbContext<MailerDbContext>();
        NewsletterSubscriberEntity subscriber = await ctx.NewsletterSubscribers.FirstAsync(s =>
            s.Email == "pending@example.com"
        );
        subscriber.Status.Should().Be(EnumNewsletterStatus.Subscribed);

        var outbox = await ctx.OutboxEmails.Where(o => o.RecipientAddress == "pending@example.com").ToListAsync();
        outbox.Should().ContainSingle(o => o.Template == "NewsletterWelcome");
        outbox[0].HtmlBody.Should().Contain(subscriber.UnsubscribeToken);
    }

    [Fact]
    public async Task Confirm_ReClick_IsIdempotentAndSendsNoSecondWelcome()
    {
        NewsletterSubscriberEntity seeded = await SeedAsync<MailerDbContext, NewsletterSubscriberEntity>(ctx =>
        {
            var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "twice@example.com");
            ctx.NewsletterSubscribers.Add(subscriber);
            return subscriber;
        });

        await Client.GetAsync($"{ApiRoutes.Public.Newsletter}/confirm/{seeded.ConfirmationToken}");
        var second = await Client.GetAsync($"{ApiRoutes.Public.Newsletter}/confirm/{seeded.ConfirmationToken}");

        second.StatusCode.Should().Be(HttpStatusCode.OK);

        await using MailerDbContext ctx = CreateDbContext<MailerDbContext>();
        int welcomeCount = await ctx.OutboxEmails.CountAsync(o =>
            o.RecipientAddress == "twice@example.com" && o.Template == "NewsletterWelcome"
        );
        welcomeCount.Should().Be(1);
    }

    [Fact]
    public async Task Confirm_UnknownToken_ReturnsNotFound()
    {
        var response = await Client.GetAsync($"{ApiRoutes.Public.Newsletter}/confirm/does-not-exist");

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }
}
