using _116.Mailer.Application.Newsletter.UseCases.Admin.Queries.GetNewsletterSubscribers.V1;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;

namespace _116.Integration.Tests.Modules.Mailer.Application.Newsletter.UseCases.Admin.Queries.GetNewsletterSubscribers.V1;

/// <summary>
/// Integration tests for the AdminGetNewsletterSubscribers endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetNewsletterSubscribersEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetSubscribers_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Admin.NewsletterSubscribers);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSubscribers_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ApiRoutes.Admin.NewsletterSubscribers);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetSubscribers_AsAdmin_ReturnsPageNewestFirstWithStatusFilter()
    {
        await SeedAsync<MailerDbContext>(ctx =>
        {
            var pending = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "pending@example.com");
            var subscribed = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "active@example.com");
            subscribed.Confirm(DateTime.UtcNow);
            ctx.NewsletterSubscribers.AddRange(pending, subscribed);
        });

        Client.AuthenticateAsAdmin();

        var all = await Client.GetAsync(ApiRoutes.Admin.NewsletterSubscribers);
        all.StatusCode.Should().Be(HttpStatusCode.OK);
        AdminGetNewsletterSubscribersResponse allBody = await all.ReadAsAsync<AdminGetNewsletterSubscribersResponse>();
        allBody.Subscribers.Count.Should().Be(2);

        var filtered = await Client.GetAsync($"{ApiRoutes.Admin.NewsletterSubscribers}?status=Subscribed");
        AdminGetNewsletterSubscribersResponse filteredBody =
            await filtered.ReadAsAsync<AdminGetNewsletterSubscribersResponse>();
        filteredBody.Subscribers.Count.Should().Be(1);
        filteredBody.Subscribers.Items.Single().Email.Should().Be("active@example.com");
    }
}
