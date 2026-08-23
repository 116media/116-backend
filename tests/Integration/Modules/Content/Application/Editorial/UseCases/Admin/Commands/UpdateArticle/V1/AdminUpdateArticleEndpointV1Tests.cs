using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle.V1;

/// <summary>
/// Integration tests for the AdminUpdateArticle endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

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

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Article"))
        );
    }

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

    [Fact]
    public async Task UpdateArticle_WithTitleTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder()
            .WithTitle(new string('A', 200))
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "Title",
                Localized<ArticleErrorMessage>(m => m.TitleTooLong(ContentConstants.MaxTitleLength))
            )
        );
    }

    [Fact]
    public async Task UpdateArticle_WithInvalidSlug_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder().WithSlug("INVALID SLUG!!!").Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Slug", Localized<ArticleErrorMessage>(m => m.SlugInvalidFormat()))
        );
    }

    [Fact]
    public async Task UpdateArticle_WithTooShortHeadline_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder().WithHeadline("Too short").Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "Headline",
                Localized<ArticleErrorMessage>(m => m.HeadlineTooShort(ContentConstants.MinHeadlineLength))
            )
        );
    }

    [Fact]
    public async Task UpdateArticle_WithSlugTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder()
            .WithSlug(new string('a', 300))
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Slug", Localized<ArticleErrorMessage>(m => m.SlugTooLong(ContentConstants.MaxSlugLength)))
        );
    }

    [Fact]
    public async Task UpdateArticle_WithHeadlineTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder()
            .WithHeadline(new string('H', 600))
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "Headline",
                Localized<ArticleErrorMessage>(m => m.HeadlineTooLong(ContentConstants.MaxHeadlineLength))
            )
        );
    }

    [Fact]
    public async Task UpdateArticle_WithMetaTitleTooShort_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder().WithMetaTitle("Short").Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "MetaTitle",
                Localized<ArticleErrorMessage>(m => m.MetaTitleTooShort(ContentConstants.MinMetaTitleLength))
            )
        );
    }

    [Fact]
    public async Task UpdateArticle_WithMetaTitleTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder()
            .WithMetaTitle(new string('M', 100))
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "MetaTitle",
                Localized<ArticleErrorMessage>(m => m.MetaTitleTooLong(ContentConstants.MaxMetaTitleLength))
            )
        );
    }

    [Fact]
    public async Task UpdateArticle_WithMetaDescriptionTooShort_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder()
            .WithMetaDescription("Too short")
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "MetaDescription",
                Localized<ArticleErrorMessage>(m =>
                    m.MetaDescriptionTooShort(ContentConstants.MinMetaDescriptionLength)
                )
            )
        );
    }

    [Fact]
    public async Task UpdateArticle_WithMetaDescriptionTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder()
            .WithMetaDescription(new string('D', 200))
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "MetaDescription",
                Localized<ArticleErrorMessage>(m => m.MetaDescriptionTooLong(ContentConstants.MaxMetaDescriptionLength))
            )
        );
    }

    [Fact]
    public async Task UpdateArticle_WithEmptyBody_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        AdminUpdateArticleRequest request = new AdminUpdateArticleRequestBuilder().WithBody(string.Empty).Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Body", Localized<ArticleErrorMessage>(m => m.BodyRequired()))
        );
    }
}
