using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetArticlePromotionFeed.V1;

/// <summary>
/// Integration tests for the PublicGetArticlePromotionFeed endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetArticlePromotionFeedEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetArticlePromotionFeed_AsAnonymous_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/promotion/feed");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the promotion feed endpoint returns OK when called as a visitor,
    /// exercising the GossipArticleSpecification internally.
    /// </summary>
    [Fact]
    public async Task GetPromotionFeed_AsVisitor_ReturnsOk()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/promotion/feed");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the promotion feed endpoint returns OK when a gossip category with
    /// published articles exists in the database. Seeding the gossip category causes the
    /// handler to call <c>GetGossipCategoryAsync</c> which internally uses
    /// <c>GossipCategorySpecification</c>, and the subsequent article query uses
    /// <c>GossipArticleSpecification</c>, bringing both specifications to 100% coverage.
    /// </summary>
    [Fact]
    public async Task GetArticlePromotionFeed_WithGossipArticles_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var gossipCategory = CategoryFactory.CreateGossip(contentType.Id);
        seedContext.Categories.Add(gossipCategory);
        await seedContext.SaveChangesAsync();

        var publishedArticle = ArticleFactory.CreatePublished(gossipCategory.Id);
        seedContext.Articles.Add(publishedArticle);
        await seedContext.SaveChangesAsync();

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/promotion/feed");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
