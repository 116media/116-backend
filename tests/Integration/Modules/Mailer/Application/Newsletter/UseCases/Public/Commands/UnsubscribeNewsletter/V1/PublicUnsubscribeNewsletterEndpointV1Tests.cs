using _116.Mailer.Application.Newsletter.UseCases.Public.Commands.UnsubscribeNewsletter.V1;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using _116.Mailer.Infrastructure.Persistence;

namespace _116.Integration.Tests.Modules.Mailer.Application.Newsletter.UseCases.Public.Commands.UnsubscribeNewsletter.V1;

/// <summary>
/// Integration tests for the PublicUnsubscribeNewsletter endpoint.
/// </summary>
[Collection("Database")]
public class PublicUnsubscribeNewsletterEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task Unsubscribe_SubscribedToken_OptsOutAndSendsNothing()
    {
        NewsletterSubscriberEntity seeded = await SeedAsync<MailerDbContext, NewsletterSubscriberEntity>(ctx =>
        {
            var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "leaving@example.com");
            subscriber.Confirm(DateTime.UtcNow);
            ctx.NewsletterSubscribers.Add(subscriber);
            return subscriber;
        });

        var response = await Client.GetAsync($"{ApiRoutes.Public.Newsletter}/unsubscribe/{seeded.UnsubscribeToken}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicUnsubscribeNewsletterResponse body = await response.ReadAsAsync<PublicUnsubscribeNewsletterResponse>();
        body.IsUnsubscribed.Should().BeTrue();

        await using MailerDbContext ctx = CreateDbContext<MailerDbContext>();
        NewsletterSubscriberEntity subscriber = await ctx.NewsletterSubscribers.FirstAsync(s =>
            s.Email == "leaving@example.com"
        );
        subscriber.Status.Should().Be(EnumNewsletterStatus.Unsubscribed);

        // Opting out never triggers an email back.
        (await ctx.OutboxEmails.CountAsync(o => o.RecipientAddress == "leaving@example.com"))
            .Should()
            .Be(0);
    }

    [Fact]
    public async Task Unsubscribe_ReClick_IsIdempotent()
    {
        NewsletterSubscriberEntity seeded = await SeedAsync<MailerDbContext, NewsletterSubscriberEntity>(ctx =>
        {
            var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "gone@example.com");
            subscriber.Confirm(DateTime.UtcNow);
            ctx.NewsletterSubscribers.Add(subscriber);
            return subscriber;
        });

        await Client.GetAsync($"{ApiRoutes.Public.Newsletter}/unsubscribe/{seeded.UnsubscribeToken}");
        var second = await Client.GetAsync($"{ApiRoutes.Public.Newsletter}/unsubscribe/{seeded.UnsubscribeToken}");

        second.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicUnsubscribeNewsletterResponse body = await second.ReadAsAsync<PublicUnsubscribeNewsletterResponse>();
        body.IsUnsubscribed.Should().BeTrue();
    }

    [Fact]
    public async Task Unsubscribe_UnknownToken_ReturnsNotFound()
    {
        var response = await Client.GetAsync($"{ApiRoutes.Public.Newsletter}/unsubscribe/nope");

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }
}
