using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllArticles.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetAllArticles.V1;

/// <summary>
/// Integration tests for the AdminGetAllArticles endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllArticlesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task GetAllArticles_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Admin.Articles);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllArticles_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ApiRoutes.Admin.Articles);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllArticles_AsAdmin_IsAllowed()
    {
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.Create(categoryId);
            ctx.Articles.Add(entity);
            return entity;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(ApiRoutes.Admin.Articles);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllArticlesResponse body = await response.ReadAsAsync<AdminGetAllArticlesResponse>();
        body.Articles.Items.Should().Contain(item => item.Id == article.Id);
        body.Articles.Count.Should().Be(1);
        body.Articles.PageIndex.Should().Be(0);
        body.Articles.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAllArticles_WithSearch_ReturnsOk()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Articles}?search=test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllArticlesResponse body = await response.ReadAsAsync<AdminGetAllArticlesResponse>();
        body.Articles.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllArticles_WithSearchQuery_ReturnsFilteredResults()
    {
        Guid categoryId = await SeedCategoryAsync();

        ArticleEntity matchingArticle = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.Create(
                categoryId,
                "UniqueSearchTerm Article",
                "unique-search-term-article"
            );
            ctx.Articles.Add(entity);
            return entity;
        });
        ArticleEntity otherArticle = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.Create(
                categoryId,
                "Completely Different Title",
                "completely-different-title"
            );
            ctx.Articles.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Articles}?search=UniqueSearchTerm");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllArticlesResponse body = await response.ReadAsAsync<AdminGetAllArticlesResponse>();
        body.Articles.Items.Should().Contain(item => item.Id == matchingArticle.Id);
        body.Articles.Items.Should().NotContain(item => item.Id == otherArticle.Id);
        body.Articles.Items.Should().OnlyContain(item => item.Title.Contains("UniqueSearchTerm"));
    }
}
