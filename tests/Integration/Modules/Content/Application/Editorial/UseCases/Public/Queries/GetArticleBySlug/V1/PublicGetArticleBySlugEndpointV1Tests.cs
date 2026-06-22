using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticleBySlug.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetArticleBySlug.V1;

/// <summary>
/// Integration tests for the PublicGetArticleBySlug endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetArticleBySlugEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task GetArticleBySlug_WithPublishedArticle_ReturnsArticle()
    {
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.CreatePublished(categoryId);
            ctx.Articles.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/{article.Slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetArticleBySlugResponse body = await response.ReadAsAsync<PublicGetArticleBySlugResponse>();
        body.Article.Id.Should().Be(article.Id);
        body.Article.Slug.Should().Be(article.Slug);
        body.Article.Title.Should().Be(article.Title);
    }

    /// <summary>
    /// Verifies that a non-published (draft) article is excluded from public reads
    /// and the endpoint reports it as not found.
    /// </summary>
    [Fact]
    public async Task GetArticleBySlug_WithDraftArticle_ReturnsNotFound()
    {
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.Create(categoryId);
            ctx.Articles.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/{article.Slug}");

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetArticleBySlug_WithNonExistent_ReturnsNotFound()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/non-existent-slug");

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }
}
