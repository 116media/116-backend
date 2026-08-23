using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleById.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleById.V1;

/// <summary>
/// Integration tests for the AdminGetArticleById endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetArticleByIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task GetArticleById_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetArticleById_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetArticleById_AsAdmin_IsAllowed()
    {
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.Create(categoryId);
            ctx.Articles.Add(entity);
            return entity;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Articles}/{article.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetArticleByIdResponse body = await response.ReadAsAsync<AdminGetArticleByIdResponse>();
        body.Article.Id.Should().Be(article.Id);
        body.Article.Title.Should().Be(article.Title);
        body.Article.Slug.Should().Be(article.Slug);
        body.Article.CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public async Task GetArticleById_WithNonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Article"))
        );
    }
}
