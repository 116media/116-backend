using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle.V1;

/// <summary>
/// Integration tests for the AdminCreateArticle endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreateArticle_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Articles, new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateArticle_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Articles, new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateArticle_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Articles, new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateArticle_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var slug = $"test-article-{Guid.NewGuid().ToString("N")[..8]}";
        var request = new
        {
            CategoryId = category.Id,
            Title = "Test Article Title",
            Slug = slug,
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Articles, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateArticle_AsSuperAdmin_WithNonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            CategoryId = Guid.NewGuid(),
            Title = "Test Article Title",
            Slug = "non-existent-category-article",
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Articles, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateArticle_AsSuperAdmin_WithEmptyTitle_ReturnsValidationError()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            CategoryId = category.Id,
            Title = "",
            Slug = "empty-title-article",
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Articles, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateArticle_AsSuperAdmin_WithDuplicateSlug_ReturnsConflict()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var existingArticle = ArticleFactory.Create(category.Id);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.Articles.Add(existingArticle);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            CategoryId = category.Id,
            Title = "Duplicate Slug Article",
            Slug = existingArticle.Slug,
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Articles, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);
    }
}
