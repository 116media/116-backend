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
}
