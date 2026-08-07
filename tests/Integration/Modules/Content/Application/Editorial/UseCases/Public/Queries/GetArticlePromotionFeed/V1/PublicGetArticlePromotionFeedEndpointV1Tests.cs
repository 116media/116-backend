using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticlePromotionFeed.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetArticlePromotionFeed.V1;

/// <summary>
/// Integration tests for the PublicGetArticlePromotionFeed endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetArticlePromotionFeedEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string FeedUrl => $"{ApiRoutes.Public.Articles}/{EditorialRouteConstants.PromotionFeed}";

    [Fact]
    public async Task GetArticlePromotionFeed_AsAnonymous_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(FeedUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetArticlePromotionFeedResponse body =
            await response.ReadAsAsync<PublicGetArticlePromotionFeedResponse>();
        body.Spot1.SpotPriority.Should().Be(EditorialFeedConstants.Spot1);
        body.Spot2.SpotPriority.Should().Be(EditorialFeedConstants.Spot2);
        body.Spot3.SpotPriority.Should().Be(EditorialFeedConstants.Spot3);
    }

    [Fact]
    public async Task GetPromotionFeed_AsVisitor_ReturnsOk()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(FeedUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetArticlePromotionFeedResponse body =
            await response.ReadAsAsync<PublicGetArticlePromotionFeedResponse>();
        body.Spot1.SpotPriority.Should().Be(EditorialFeedConstants.Spot1);
    }

    [Fact]
    public async Task GetArticlePromotionFeed_WithGossipArticles_ReturnsOk()
    {
        Guid gossipCategoryId = await SeedAsync<ContentDbContext, Guid>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity gossipCategory = CategoryFactory.CreateGossip(contentType.Id);
            ctx.Categories.Add(gossipCategory);

            return gossipCategory.Id;
        });

        ArticleEntity publishedArticle = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.CreatePublished(gossipCategoryId);
            ctx.Articles.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(FeedUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetArticlePromotionFeedResponse body =
            await response.ReadAsAsync<PublicGetArticlePromotionFeedResponse>();

        IEnumerable<ArticleSummaryDto> allFeedArticles = body
            .Spot1.Articles.Concat(body.Spot2.Articles)
            .Concat(body.Spot3.Slots.SelectMany(slot => slot.Articles))
            .Concat(body.GossipStrip);

        allFeedArticles.Should().Contain(item => item.Id == publishedArticle.Id);
    }

    [Fact]
    public async Task GetArticlePromotionFeed_WhenAuthenticated_StampsOnlyTheUsersInteractions()
    {
        Guid gossipCategoryId = await SeedAsync<ContentDbContext, Guid>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity gossipCategory = CategoryFactory.CreateGossip(contentType.Id);
            ctx.Categories.Add(gossipCategory);

            return gossipCategory.Id;
        });

        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        ArticleEntity likedArticle = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.CreatePublished(gossipCategoryId);
            ctx.Articles.Add(entity);
            ctx.ArticleLikes.Add(ArticleLikeEntity.Create(Guid.NewGuid(), userId, entity.Id));
            ctx.ArticleBookmarks.Add(ArticleBookmarkEntity.Create(Guid.NewGuid(), otherUserId, entity.Id));
            return entity;
        });

        Client.AuthenticateAs(userId, "Visitor");

        var response = await Client.GetAsync(FeedUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetArticlePromotionFeedResponse body =
            await response.ReadAsAsync<PublicGetArticlePromotionFeedResponse>();

        List<ArticleSummaryDto> allFeedArticles = body
            .Spot1.Articles.Concat(body.Spot2.Articles)
            .Concat(body.Spot3.Slots.SelectMany(slot => slot.Articles))
            .Concat(body.GossipStrip)
            .ToList();

        ArticleSummaryDto stamped = allFeedArticles.Single(item => item.Id == likedArticle.Id);
        stamped.IsLiked.Should().BeTrue();
        stamped.IsBookmarked.Should().BeFalse();
    }
}
