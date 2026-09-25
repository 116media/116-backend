using _116.Content.Application.Editorial.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// End-to-end proof that the four customer-facing emails a commissioned piece produces are
/// queued over real HTTP: publishing, rejecting, unpromoting and scheduling a shoot each
/// reach the paying customer through the post-commit event path.
/// </summary>
[Collection("Database")]
public class CommissionedContentEmailFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// Seeds the category graph and a customer the commissioned content bills to.
    /// </summary>
    /// <returns>The category the content sits in and the paying customer.</returns>
    private async Task<(CategoryEntity Category, CustomerEntity Customer)> SeedCommissionAsync()
    {
        CustomerEntity customer = CustomerFactory.CreateWithId(Guid.NewGuid());
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);

        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
        });

        return (category, customer);
    }

    /// <summary>
    /// Reads the single outbox row queued for an address under a template.
    /// </summary>
    /// <param name="email">The recipient address.</param>
    /// <param name="template">The expected template name.</param>
    /// <returns>The queued row.</returns>
    private async Task<OutboxEmailEntity> QueuedAsync(string email, string template)
    {
        await using MailerDbContext mailer = CreateDbContext<MailerDbContext>();

        List<OutboxEmailEntity> rows = await mailer
            .OutboxEmails.Where(row => row.RecipientAddress == email)
            .ToListAsync();

        return rows.Should().ContainSingle(row => row.Template == template).Which;
    }

    [Fact]
    public async Task PublishArticle_WhenCommissioned_QueuesThePublishedEmailToTheCustomer()
    {
        (CategoryEntity category, CustomerEntity customer) = await SeedCommissionAsync();
        ArticleEntity article = new ArticleBuilder(category.Id)
            .WithCustomer(customer.Id, Guid.NewGuid())
            .AsApproved()
            .Build();
        await SeedAsync<ContentDbContext>(ctx => ctx.Articles.Add(article));

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Articles, article.Id),
            content: null
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        OutboxEmailEntity queued = await QueuedAsync(customer.Email, "CommissionedContentPublished");
        queued.TextBody.Should().Contain(article.Title);
    }

    [Fact]
    public async Task PublishArticle_WhenNotCommissioned_QueuesNothingForAnyCustomer()
    {
        (CategoryEntity category, CustomerEntity customer) = await SeedCommissionAsync();
        ArticleEntity article = ArticleFactory.CreateApproved(category.Id);
        await SeedAsync<ContentDbContext>(ctx => ctx.Articles.Add(article));

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Articles, article.Id),
            content: null
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using MailerDbContext mailer = CreateDbContext<MailerDbContext>();
        (await mailer.OutboxEmails.CountAsync(row => row.RecipientAddress == customer.Email)).Should().Be(0);
    }

    [Fact]
    public async Task RejectArticle_WhenCommissioned_QueuesTheRejectedEmailToTheCustomer()
    {
        (CategoryEntity category, CustomerEntity customer) = await SeedCommissionAsync();
        ArticleEntity article = new ArticleBuilder(category.Id)
            .WithCustomer(customer.Id, Guid.NewGuid())
            .AsPendingReview()
            .Build();
        await SeedAsync<ContentDbContext>(ctx => ctx.Articles.Add(article));

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Articles, article.Id),
            new { Reason = "Needs a stronger lede before it can run." }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        OutboxEmailEntity queued = await QueuedAsync(customer.Email, "CommissionedContentRejected");
        queued.TextBody.Should().Contain(article.Title);
    }

    [Fact]
    public async Task ForceUnpromoteArticle_WhenCommissioned_QueuesTheRemovalEmailToTheCustomer()
    {
        (CategoryEntity category, CustomerEntity customer) = await SeedCommissionAsync();
        PromotionLevelEntity promoLevel = PromotionLevelFactory.CreateDefault();
        ArticleEntity article = new ArticleBuilder(category.Id)
            .WithCustomer(customer.Id, Guid.NewGuid())
            .AsPublished()
            .AsPromoted(DateTimeOffset.UtcNow.AddDays(7), promoLevel.Id)
            .Build();
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.PromotionLevels.Add(promoLevel);
            ctx.Articles.Add(article);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}/{article.Slug}/{EditorialRouteConstants.Unpromote}",
            new { Reason = "The campaign window closed early." }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        OutboxEmailEntity queued = await QueuedAsync(customer.Email, "PromotionForceRemoved");
        queued.TextBody.Should().Contain(article.Title).And.Contain("The campaign window closed early.");
    }

    [Fact]
    public async Task ScheduleShoot_WhenCommissioned_QueuesTheShootEmailToTheCustomer()
    {
        (CategoryEntity category, CustomerEntity customer) = await SeedCommissionAsync();
        VideoEntity video = new VideoBuilder(category.Id).WithCustomer(customer.Id, Guid.NewGuid()).Build();
        await SeedAsync<ContentDbContext>(ctx => ctx.Videos.Add(video));

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Shoot(EditorialRouteConstants.Videos, video.Id),
            new { ShootingScheduledAt = DateTimeOffset.UtcNow.AddDays(14) }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        OutboxEmailEntity queued = await QueuedAsync(customer.Email, "ShootScheduled");
        queued.TextBody.Should().Contain(video.Title);
    }
}
