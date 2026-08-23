using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles.V1;

/// <summary>
/// Integration tests for the PublicGetPublishedArticles endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetPublishedArticlesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<Guid> SeedCategoryAsync()
    {
        return await SeedAsync<ContentDbContext, Guid>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            return category.Id;
        });
    }

    [Fact]
    public async Task GetPublishedArticles_AsAnonymous_ReturnsOk()
    {
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity publishedArticle = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.CreatePublished(categoryId);
            ctx.Articles.Add(entity);
            return entity;
        });
        ArticleEntity draftArticle = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.Create(categoryId);
            ctx.Articles.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.Articles);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedArticlesResponse body = await response.ReadAsAsync<PublicGetPublishedArticlesResponse>();
        body.Articles.Items.Should().Contain(item => item.Id == publishedArticle.Id);
        body.Articles.Items.Should().NotContain(item => item.Id == draftArticle.Id);
        body.Articles.Items.Should().OnlyContain(item => item.Status == EnumContentStatus.Published);
        body.Articles.PageIndex.Should().Be(0);
        body.Articles.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetPublishedArticles_WhenAuthenticated_StampsOnlyTheUsersInteractions()
    {
        Guid categoryId = await SeedCategoryAsync();
        Guid userId = Guid.NewGuid();
        List<ArticleEntity> articles = await SeedAsync<ContentDbContext, List<ArticleEntity>>(ctx =>
        {
            List<ArticleEntity> entities = ArticleFactory.CreateManyPublished(categoryId, 3);
            ctx.Articles.AddRange(entities);
            ctx.ArticleLikes.Add(ArticleLikeEntity.Create(Guid.NewGuid(), userId, entities[0].Id));
            ctx.ArticleBookmarks.Add(ArticleBookmarkEntity.Create(Guid.NewGuid(), userId, entities[1].Id));
            return entities;
        });

        Client.AuthenticateAs(userId, "Visitor");

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}?categoryId={categoryId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedArticlesResponse body = await response.ReadAsAsync<PublicGetPublishedArticlesResponse>();
        body.Articles.Items.Single(item => item.Id == articles[0].Id).IsLiked.Should().BeTrue();
        body.Articles.Items.Single(item => item.Id == articles[0].Id).IsBookmarked.Should().BeFalse();
        body.Articles.Items.Single(item => item.Id == articles[1].Id).IsBookmarked.Should().BeTrue();
        body.Articles.Items.Single(item => item.Id == articles[1].Id).IsLiked.Should().BeFalse();
        body.Articles.Items.Single(item => item.Id == articles[2].Id).IsLiked.Should().BeFalse();
        body.Articles.Items.Single(item => item.Id == articles[2].Id).IsBookmarked.Should().BeFalse();
    }

    [Fact]
    public async Task GetPublishedArticles_DoesNotLeakInteractionStateAcrossUsers()
    {
        Guid categoryId = await SeedCategoryAsync();
        Guid userAId = Guid.NewGuid();
        Guid userBId = Guid.NewGuid();
        ArticleEntity likedArticle = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.CreatePublished(categoryId);
            ctx.Articles.Add(entity);
            ctx.ArticleLikes.Add(ArticleLikeEntity.Create(Guid.NewGuid(), userAId, entity.Id));
            return entity;
        });

        Client.AuthenticateAs(userAId, "Visitor");
        var responseA = await Client.GetAsync($"{ApiRoutes.Public.Articles}?categoryId={categoryId}");
        PublicGetPublishedArticlesResponse bodyA = await responseA.ReadAsAsync<PublicGetPublishedArticlesResponse>();
        bodyA.Articles.Items.Single(item => item.Id == likedArticle.Id).IsLiked.Should().BeTrue();

        Client.AuthenticateAs(userBId, "Visitor");
        var responseB = await Client.GetAsync($"{ApiRoutes.Public.Articles}?categoryId={categoryId}");
        PublicGetPublishedArticlesResponse bodyB = await responseB.ReadAsAsync<PublicGetPublishedArticlesResponse>();
        bodyB.Articles.Items.Single(item => item.Id == likedArticle.Id).IsLiked.Should().BeFalse();
    }
}
