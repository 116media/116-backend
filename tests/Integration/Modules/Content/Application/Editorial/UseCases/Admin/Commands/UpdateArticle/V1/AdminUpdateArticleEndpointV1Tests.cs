using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle.V1;

/// <summary>
/// Integration tests for the AdminUpdateArticle endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UpdateArticle_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateArticle_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateArticle_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}", request);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that a full article update returns 200 OK, echoes the new title/headline/body
    /// in the typed response, and persists the changed fields.
    /// </summary>
    [Fact]
    public async Task UpdateArticle_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArticleEntity a = ArticleFactory.Create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Articles.Add(a);
            return a;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder()
            .WithCategoryId(article.CategoryId)
            .WithTitle("Updated Article Title")
            .WithSlug(article.Slug)
            .WithBody("<p>Updated article body content.</p>")
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{article.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminUpdateArticleResponse>();
        body.Article.Id.Should().Be(article.Id);
        body.Article.Title.Should().Be(request.Title);
        body.Article.Headline.Should().Be(request.Headline);
        body.Article.Body.Should().Be(request.Body);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArticleEntity? persisted = await ctx.Articles.FindAsync(article.Id);
        persisted.Should().NotBeNull();
        persisted!.Title.Should().Be(request.Title);
        persisted.Headline.Should().Be(request.Headline);
        persisted.Body.Should().Be(request.Body);
    }

    /// <summary>
    /// Verifies that updating an article with a title exceeding the maximum allowed length
    /// (100 characters) returns a 400 Bad Request response from the validator, exercising
    /// the <c>isRequired=false</c> branch of <c>ValidArticleTitle</c> in EditorialValidation.
    /// </summary>
    [Fact]
    public async Task UpdateArticle_WithTitleTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder()
            .WithTitle(new string('A', 200))
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that updating an article with a slug containing spaces and special characters
    /// returns a 400 Bad Request response from the validator, exercising the slug regex
    /// validation in EditorialValidation.
    /// </summary>
    [Fact]
    public async Task UpdateArticle_WithInvalidSlug_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder().WithSlug("INVALID SLUG!!!").Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that updating an article with a headline shorter than the minimum allowed
    /// length (100 characters) returns a 400 Bad Request response from the validator.
    /// </summary>
    [Fact]
    public async Task UpdateArticle_WithTooShortHeadline_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder().WithHeadline("Too short").Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that updating an article with a slug exceeding the maximum allowed length
    /// (220 characters) returns a 400 Bad Request response from the validator, exercising
    /// the MaximumLength branch of <c>ValidArticleSlug</c> in EditorialValidation.
    /// </summary>
    [Fact]
    public async Task UpdateArticle_WithSlugTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder()
            .WithSlug(new string('a', 300))
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that updating an article with a headline exceeding the maximum allowed length
    /// (500 characters) returns a 400 Bad Request response from the validator, exercising
    /// the MaximumLength branch of <c>ValidArticleHeadline</c> in EditorialValidation.
    /// </summary>
    [Fact]
    public async Task UpdateArticle_WithHeadlineTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder()
            .WithHeadline(new string('H', 600))
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that updating an article with a meta title shorter than the minimum allowed
    /// length (10 characters) returns a 400 Bad Request response from the validator,
    /// exercising the MinimumLength branch of <c>ValidMetaTitle</c> in EditorialValidation.
    /// </summary>
    [Fact]
    public async Task UpdateArticle_WithMetaTitleTooShort_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder().WithMetaTitle("Short").Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that updating an article with a meta title exceeding the maximum allowed
    /// length (70 characters) returns a 400 Bad Request response from the validator,
    /// exercising the MaximumLength branch of <c>ValidMetaTitle</c> in EditorialValidation.
    /// </summary>
    [Fact]
    public async Task UpdateArticle_WithMetaTitleTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder()
            .WithMetaTitle(new string('M', 100))
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that updating an article with a meta description shorter than the minimum
    /// allowed length (50 characters) returns a 400 Bad Request response from the validator,
    /// exercising the MinimumLength branch of <c>ValidMetaDescription</c> in EditorialValidation.
    /// </summary>
    [Fact]
    public async Task UpdateArticle_WithMetaDescriptionTooShort_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder()
            .WithMetaDescription("Too short")
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that updating an article with a meta description exceeding the maximum allowed
    /// length (160 characters) returns a 400 Bad Request response from the validator,
    /// exercising the MaximumLength branch of <c>ValidMetaDescription</c> in EditorialValidation.
    /// </summary>
    [Fact]
    public async Task UpdateArticle_WithMetaDescriptionTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder()
            .WithMetaDescription(new string('D', 200))
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that updating an article with an empty body returns a 400 Bad Request response
    /// from the validator, exercising the <c>ValidArticleBody</c> branch in EditorialValidation.
    /// </summary>
    [Fact]
    public async Task UpdateArticle_WithEmptyBody_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder().WithBody(string.Empty).Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }
}
