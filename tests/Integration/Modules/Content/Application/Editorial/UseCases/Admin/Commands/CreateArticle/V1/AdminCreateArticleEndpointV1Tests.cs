using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle.V1;

/// <summary>
/// Integration tests for the AdminCreateArticle endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    private async Task<CategoryEntity> SeedCategoryAsync()
    {
        return await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            return category;
        });
    }

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
        CategoryEntity category = await SeedCategoryAsync();

        Client.AuthenticateAsSuperAdmin();
        string slug = $"test-article-{Guid.NewGuid().ToString("N")[..8]}";
        AdminCreateArticleRequest request = new AdminCreateArticleRequestBuilder()
            .WithCategoryId(category.Id)
            .WithSlug(slug)
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Articles, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.ReadAsAsync<AdminCreateArticleResponse>();
        body.Article.Id.Should().NotBeEmpty();
        body.Article.Title.Should().Be(request.Title);
        body.Article.Slug.Should().Be(request.Slug);
        body.Article.CategoryId.Should().Be(category.Id);
        body.Article.Status.Should().Be(EnumContentStatus.Draft);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArticleEntity? persisted = await ctx.Articles.FindAsync(body.Article.Id);
        persisted.Should().NotBeNull();
        persisted!.Title.Should().Be(request.Title);
        persisted.Slug.Should().Be(request.Slug);
        persisted.Status.Should().Be(EnumContentStatus.Draft);
    }

    [Fact]
    public async Task CreateArticle_AsSuperAdmin_WithNonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminCreateArticleRequest request = new AdminCreateArticleRequestBuilder()
            .WithCategoryId(Guid.NewGuid())
            .WithSlug("non-existent-category-article")
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Articles, request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Category"))
        );
    }

    [Fact]
    public async Task CreateArticle_AsSuperAdmin_WithEmptyTitle_ReturnsValidationError()
    {
        CategoryEntity category = await SeedCategoryAsync();

        Client.AuthenticateAsSuperAdmin();
        AdminCreateArticleRequest request = new AdminCreateArticleRequestBuilder()
            .WithCategoryId(category.Id)
            .WithTitle(string.Empty)
            .WithSlug("empty-title-article")
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Articles, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Title", Localized<ArticleErrorMessage>(m => m.TitleRequired()))
        );
    }

    [Fact]
    public async Task CreateArticle_AsSuperAdmin_WithDuplicateSlug_ReturnsConflict()
    {
        ArticleEntity existingArticle = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArticleEntity article = ArticleFactory.Create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Articles.Add(article);
            return article;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminCreateArticleRequest request = new AdminCreateArticleRequestBuilder()
            .WithCategoryId(existingArticle.CategoryId)
            .WithSlug(existingArticle.Slug)
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Articles, request);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ArticleErrorMessage>(m => m.SlugAlreadyExists(existingArticle.Slug))
        );
    }

    [Fact]
    public async Task CreateArticle_AsSuperAdmin_WithPaidArticle_ReturnsCreated()
    {
        Guid categoryId = Guid.Empty;
        CustomerEntity customer = await SeedAsync<ContentDbContext, CustomerEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            CustomerEntity c = CustomerFactory.Create();
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Customers.Add(c);
            categoryId = category.Id;
            return c;
        });

        Client.AuthenticateAsSuperAdmin();
        string slug = $"paid-article-{Guid.NewGuid().ToString("N")[..8]}";
        Guid orderItemId = Guid.NewGuid();
        AdminCreateArticleRequest request = new AdminCreateArticleRequestBuilder()
            .WithCategoryId(categoryId)
            .WithSlug(slug)
            .WithCustomerId(customer.Id)
            .WithOrderItemId(orderItemId)
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Articles, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.ReadAsAsync<AdminCreateArticleResponse>();
        body.Article.Id.Should().NotBeEmpty();
        body.Article.CustomerId.Should().Be(customer.Id);
        body.Article.OrderItemId.Should().Be(orderItemId);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArticleEntity? persisted = await ctx.Articles.FindAsync(body.Article.Id);
        persisted.Should().NotBeNull();
        persisted!.CustomerId.Should().Be(customer.Id);
        persisted.OrderItemId.Should().Be(orderItemId);
    }
}
