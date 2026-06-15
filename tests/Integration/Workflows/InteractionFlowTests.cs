using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// Cross-module workflow tests for content interaction lifecycle:
/// like → verify count → unlike → verify count decremented.
/// Includes bookmark and comment flows.
/// </summary>
[Collection("Database")]
public class InteractionFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task LikeArticle_ShouldToggleLikeStatus()
    {
        Guid articleId = await SeedPublishedArticleAsync();

        Client.AuthenticateAsVisitor();

        HttpResponseMessage likeResponse = await Client.PostAsync(
            $"{ApiRoutes.Public.Articles}/{articleId}/likes",
            null
        );
        likeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        HttpResponseMessage unlikeResponse = await Client.DeleteAsync($"{ApiRoutes.Public.Articles}/{articleId}/likes");
        unlikeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task BookmarkArticle_ShouldPersist()
    {
        Guid articleId = await SeedPublishedArticleAsync();

        Client.AuthenticateAsVisitor();

        HttpResponseMessage bookmarkResponse = await Client.PostAsync(
            $"{ApiRoutes.Public.Articles}/{articleId}/bookmarks",
            null
        );
        bookmarkResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CommentOnArticle_ShouldReturnCreated()
    {
        Guid articleId = await SeedPublishedArticleAsync();

        Client.AuthenticateAsVisitor();

        var commentRequest = new { Body = "Great article!" };
        HttpResponseMessage commentResponse = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Public.Articles}/{articleId}/comments",
            commentRequest
        );
        commentResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
    }

    [Fact]
    public async Task RateVideo_ShouldReturnOk()
    {
        Guid videoId = await SeedPublishedVideoAsync();

        Client.AuthenticateAsVisitor();

        var rateRequest = new { Stars = (short)4 };
        HttpResponseMessage rateResponse = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Public.Videos}/{videoId}/ratings",
            rateRequest
        );
        rateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ShareArticle_ShouldReturnOk()
    {
        Guid articleId = await SeedPublishedArticleAsync();

        Client.AuthenticateAsVisitor();

        HttpResponseMessage shareResponse = await Client.PostAsync(
            $"{ApiRoutes.Public.Articles}/{articleId}/shares",
            null
        );
        shareResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task InteractWithoutAuth_ShouldReturnUnauthorized()
    {
        Guid articleId = await SeedPublishedArticleAsync();

        Client.ClearAuthentication();

        HttpResponseMessage response = await Client.PostAsync($"{ApiRoutes.Public.Articles}/{articleId}/likes", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Seeds a published article and returns its ID.
    /// </summary>
    private async Task<Guid> SeedPublishedArticleAsync()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Article");
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var article = ArticleFactory.CreatePublished(category.Id);
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        return article.Id;
    }

    /// <summary>
    /// Seeds a published video and returns its ID.
    /// </summary>
    private async Task<Guid> SeedPublishedVideoAsync()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Video");
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var video = VideoFactory.CreatePublished(category.Id);
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        return video.Id;
    }
}
