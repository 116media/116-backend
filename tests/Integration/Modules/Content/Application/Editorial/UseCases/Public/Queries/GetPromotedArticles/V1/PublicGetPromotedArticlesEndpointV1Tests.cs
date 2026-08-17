using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedArticles.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedArticles.V1;

/// <summary>
/// Integration tests for the PublicGetPromotedArticles endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetPromotedArticlesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string PromotedUrl => $"{ApiRoutes.Public.Articles}/{EditorialRouteConstants.Promoted}";

    private async Task<(Guid CategoryId, Guid PromotionLevelId)> SeedCategoryAsync()
    {
        return await SeedAsync<ContentDbContext, (Guid, Guid)>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            PromotionLevelEntity promotionLevel = PromotionLevelFactory.Create();
            ctx.PromotionLevels.Add(promotionLevel);

            return (category.Id, promotionLevel.Id);
        });
    }

    [Fact]
    public async Task GetPromotedArticles_AsAnonymous_ReturnsOk()
    {
        (Guid categoryId, Guid promotionLevelId) = await SeedCategoryAsync();
        ArticleEntity promotedArticle = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.CreatePromoted(categoryId, promotionLevelId);
            ctx.Articles.Add(entity);
            return entity;
        });
        ArticleEntity publishedArticle = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.CreatePublished(categoryId);
            ctx.Articles.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(PromotedUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPromotedArticlesResponse body = await response.ReadAsAsync<PublicGetPromotedArticlesResponse>();
        body.Articles.Should().Contain(item => item.Id == promotedArticle.Id);
        body.Articles.Should().NotContain(item => item.Id == publishedArticle.Id);
        body.Articles.Should().OnlyContain(item => item.IsPromoted);
    }

    [Fact]
    public async Task GetPromotedArticles_WhenAuthenticated_StampsOnlyTheUsersInteractions()
    {
        (Guid categoryId, Guid promotionLevelId) = await SeedCategoryAsync();
        Guid userId = await SeedAuthenticatedUserAsync();
        Guid otherUserId = Guid.NewGuid();
        ArticleEntity interactedArticle = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.CreatePromoted(categoryId, promotionLevelId);
            ctx.Articles.Add(entity);
            ctx.ArticleLikes.Add(ArticleLikeEntity.Create(Guid.NewGuid(), userId, entity.Id));
            ctx.ArticleBookmarks.Add(ArticleBookmarkEntity.Create(Guid.NewGuid(), userId, entity.Id));
            return entity;
        });
        ArticleEntity otherUsersArticle = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.CreatePromoted(categoryId, promotionLevelId);
            ctx.Articles.Add(entity);
            ctx.ArticleLikes.Add(ArticleLikeEntity.Create(Guid.NewGuid(), otherUserId, entity.Id));
            return entity;
        });

        Client.AuthenticateAs(userId, "Visitor");

        var response = await Client.GetAsync(PromotedUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPromotedArticlesResponse body = await response.ReadAsAsync<PublicGetPromotedArticlesResponse>();
        body.Articles.Single(item => item.Id == interactedArticle.Id).IsLiked.Should().BeTrue();
        body.Articles.Single(item => item.Id == interactedArticle.Id).IsBookmarked.Should().BeTrue();
        body.Articles.Single(item => item.Id == otherUsersArticle.Id).IsLiked.Should().BeFalse();
        body.Articles.Single(item => item.Id == otherUsersArticle.Id).IsBookmarked.Should().BeFalse();
    }
}
