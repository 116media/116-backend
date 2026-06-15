using _116.Content.Infrastructure.Persistence;
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

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}", new { });

        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateArticle_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var article = ArticleFactory.Create(category.Id);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.Articles.Add(article);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            CategoryId = category.Id,
            Title = "Updated Article Title",
            Slug = article.Slug,
            Headline = "This is a test headline that is intentionally written to be long enough to pass the minimum validation length requirement of one hundred characters.",
            Body = "<p>Updated article body content.</p>",
            SocialBoost = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{article.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
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
        var request = new
        {
            CategoryId = Guid.NewGuid(),
            Title = new string('A', 200),
            Slug = "valid-slug",
            Headline = string.Empty,
            Body = string.Empty,
            SocialBoost = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
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
        var request = new
        {
            CategoryId = Guid.NewGuid(),
            Title = "Valid Title",
            Slug = "INVALID SLUG!!!",
            Headline = "This is a test headline that is intentionally written to be long enough to pass the minimum validation length requirement of one hundred characters.",
            Body = "<p>Valid body.</p>",
            SocialBoost = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
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
        var request = new
        {
            CategoryId = Guid.NewGuid(),
            Title = "Valid Title",
            Slug = "valid-slug",
            Headline = "Too short",
            Body = "<p>Valid body.</p>",
            SocialBoost = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
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
        var request = new
        {
            CategoryId = Guid.NewGuid(),
            Title = "Valid Title",
            Slug = new string('a', 300),
            Headline = "This is a test headline that is intentionally written to be long enough to pass the minimum validation length requirement of one hundred characters.",
            Body = "<p>Valid body.</p>",
            SocialBoost = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
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
        var request = new
        {
            CategoryId = Guid.NewGuid(),
            Title = "Valid Title",
            Slug = "valid-slug",
            Headline = new string('H', 600),
            Body = "<p>Valid body.</p>",
            SocialBoost = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
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
        var request = new
        {
            CategoryId = Guid.NewGuid(),
            Title = "Valid Title",
            Slug = "valid-slug",
            Headline = "This is a test headline that is intentionally written to be long enough to pass the minimum validation length requirement of one hundred characters.",
            Body = "<p>Valid body.</p>",
            MetaTitle = "Short",
            SocialBoost = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
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
        var request = new
        {
            CategoryId = Guid.NewGuid(),
            Title = "Valid Title",
            Slug = "valid-slug",
            Headline = "This is a test headline that is intentionally written to be long enough to pass the minimum validation length requirement of one hundred characters.",
            Body = "<p>Valid body.</p>",
            MetaTitle = new string('M', 100),
            SocialBoost = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
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
        var request = new
        {
            CategoryId = Guid.NewGuid(),
            Title = "Valid Title",
            Slug = "valid-slug",
            Headline = "This is a test headline that is intentionally written to be long enough to pass the minimum validation length requirement of one hundred characters.",
            Body = "<p>Valid body.</p>",
            MetaDescription = "Too short",
            SocialBoost = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
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
        var request = new
        {
            CategoryId = Guid.NewGuid(),
            Title = "Valid Title",
            Slug = "valid-slug",
            Headline = "This is a test headline that is intentionally written to be long enough to pass the minimum validation length requirement of one hundred characters.",
            Body = "<p>Valid body.</p>",
            MetaDescription = new string('D', 200),
            SocialBoost = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
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
        var request = new
        {
            CategoryId = Guid.NewGuid(),
            Title = "Valid Title",
            Slug = "valid-slug",
            Headline = "This is a test headline that is intentionally written to be long enough to pass the minimum validation length requirement of one hundred characters.",
            Body = "",
            SocialBoost = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
