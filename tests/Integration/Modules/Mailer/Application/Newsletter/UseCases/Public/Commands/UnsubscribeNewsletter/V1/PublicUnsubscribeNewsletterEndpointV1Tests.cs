using System.Net;
using _116.Mailer.Application.Newsletter.Messages;
using _116.Mailer.Application.Newsletter.UseCases.Public.Commands.UnsubscribeNewsletter.V1;
using _116.Mailer.Application.Shared.Errors.Messages;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using _116.Mailer.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;

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

        var response = await Client.PostAsync(
            $"{ApiRoutes.Public.Newsletter}/unsubscribe/{seeded.UnsubscribeToken}",
            content: null
        );

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

        await Client.PostAsync($"{ApiRoutes.Public.Newsletter}/unsubscribe/{seeded.UnsubscribeToken}", content: null);
        var second = await Client.PostAsync(
            $"{ApiRoutes.Public.Newsletter}/unsubscribe/{seeded.UnsubscribeToken}",
            content: null
        );

        second.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicUnsubscribeNewsletterResponse body = await second.ReadAsAsync<PublicUnsubscribeNewsletterResponse>();
        body.IsUnsubscribed.Should().BeTrue();
    }

    [Fact]
    public async Task Unsubscribe_UnknownToken_ReturnsNotFound()
    {
        var response = await Client.PostAsync($"{ApiRoutes.Public.Newsletter}/unsubscribe/nope", content: null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<NewsletterErrorMessage>(m => m.TokenInvalid())
        );
    }

    [Fact]
    public async Task Get_ShouldRenderAPageWithoutChangingAnything()
    {
        NewsletterSubscriberEntity seeded = await SeedAsync<MailerDbContext, NewsletterSubscriberEntity>(ctx =>
        {
            var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "scanner-unsub@example.com");
            subscriber.Confirm(DateTime.UtcNow);
            ctx.NewsletterSubscribers.Add(subscriber);
            return subscriber;
        });

        var response = await Client.GetAsync($"{ApiRoutes.Public.Newsletter}/unsubscribe/{seeded.UnsubscribeToken}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");

        string html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("<form method=\"post\"");
        html.Should().Contain(WebUtility.HtmlEncode(Localized<NewsletterPageMessage>(page => page.UnsubscribeTitle())));
        html.Should()
            .Contain(WebUtility.HtmlEncode(Localized<NewsletterPageMessage>(page => page.UnsubscribeButton())));

        await using MailerDbContext db = CreateDbContext<MailerDbContext>();
        NewsletterSubscriberEntity? after = await db.NewsletterSubscribers.FindAsync(seeded.Id);
        after!.Status.Should().NotBe(EnumNewsletterStatus.Unsubscribed);
    }
}
