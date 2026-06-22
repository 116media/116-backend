using _116.Content.Application.Interactions.UseCases.Public.Commands.AddArticleComment.V1;
using _116.Content.Application.Interactions.UseCases.Public.Commands.BookmarkArticle.V1;
using _116.Content.Application.Interactions.UseCases.Public.Commands.LikeArticle.V1;
using _116.Content.Application.Interactions.UseCases.Public.Commands.RateVideo.V1;
using _116.Content.Application.Interactions.UseCases.Public.Commands.ShareArticle.V1;
using _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeArticle.V1;
using _116.Content.Domain.Entities;
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

        HttpResponseMessage likeResponse = await Client.PostAsync(Routes.Public.Articles.Likes(articleId), null);
        likeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicLikeArticleResponse likeBody = await likeResponse.ReadAsAsync<PublicLikeArticleResponse>();
        likeBody.IsSuccess.Should().BeTrue();

        await using (ContentDbContext likeContext = CreateDbContext<ContentDbContext>())
        {
            (await likeContext.ArticleLikes.CountAsync(l => l.ArticleId == articleId)).Should().Be(1);
        }

        HttpResponseMessage unlikeResponse = await Client.DeleteAsync(Routes.Public.Articles.Likes(articleId));
        unlikeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicUnlikeArticleResponse unlikeBody = await unlikeResponse.ReadAsAsync<PublicUnlikeArticleResponse>();
        unlikeBody.IsSuccess.Should().BeTrue();

        await using (ContentDbContext unlikeContext = CreateDbContext<ContentDbContext>())
        {
            (await unlikeContext.ArticleLikes.CountAsync(l => l.ArticleId == articleId)).Should().Be(0);
        }
    }

    [Fact]
    public async Task BookmarkArticle_ShouldPersist()
    {
        Guid articleId = await SeedPublishedArticleAsync();

        Client.AuthenticateAsVisitor();

        HttpResponseMessage bookmarkResponse = await Client.PostAsync(
            Routes.Public.Articles.Bookmarks(articleId),
            null
        );
        bookmarkResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicBookmarkArticleResponse bookmarkBody =
            await bookmarkResponse.ReadAsAsync<PublicBookmarkArticleResponse>();
        bookmarkBody.IsSuccess.Should().BeTrue();

        await using ContentDbContext context = CreateDbContext<ContentDbContext>();
        (await context.ArticleBookmarks.CountAsync(b => b.ArticleId == articleId)).Should().Be(1);
    }

    [Fact]
    public async Task CommentOnArticle_ShouldReturnCreated()
    {
        Guid articleId = await SeedPublishedArticleAsync();

        Client.AuthenticateAsVisitor();

        var commentRequest = new PublicAddArticleCommentRequest(Body: "Great article!");
        HttpResponseMessage commentResponse = await Client.PostAsJsonAsync(
            Routes.Public.Articles.Comments(articleId),
            commentRequest
        );
        commentResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        PublicAddArticleCommentResponse commentBody =
            await commentResponse.ReadAsAsync<PublicAddArticleCommentResponse>();
        commentBody.Comment.Id.Should().NotBeEmpty();
        commentBody.Comment.Body.Should().Be("Great article!");

        await using ContentDbContext context = CreateDbContext<ContentDbContext>();
        ArticleCommentEntity persisted = await context.ArticleComments.FirstAsync(c => c.Id == commentBody.Comment.Id);
        persisted.ArticleId.Should().Be(articleId);
        persisted.Body.Should().Be("Great article!");
    }

    [Fact]
    public async Task RateVideo_ShouldReturnOk()
    {
        Guid videoId = await SeedPublishedVideoAsync();

        Client.AuthenticateAsVisitor();

        var rateRequest = new PublicRateVideoRequest(Stars: 4);
        HttpResponseMessage rateResponse = await Client.PostAsJsonAsync(
            Routes.Public.Videos.Ratings(videoId),
            rateRequest
        );
        rateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicRateVideoResponse rateBody = await rateResponse.ReadAsAsync<PublicRateVideoResponse>();
        rateBody.IsSuccess.Should().BeTrue();

        await using ContentDbContext context = CreateDbContext<ContentDbContext>();
        VideoRatingEntity rating = await context.VideoRatings.FirstAsync(r => r.VideoId == videoId);
        rating.Stars.Should().Be(4);
    }

    [Fact]
    public async Task ShareArticle_ShouldReturnOk()
    {
        Guid articleId = await SeedPublishedArticleAsync();

        Client.AuthenticateAsVisitor();

        HttpResponseMessage shareResponse = await Client.PostAsync(Routes.Public.Articles.Shares(articleId), null);
        shareResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicShareArticleResponse shareBody = await shareResponse.ReadAsAsync<PublicShareArticleResponse>();
        shareBody.IsSuccess.Should().BeTrue();

        await using ContentDbContext context = CreateDbContext<ContentDbContext>();
        (await context.ArticleShares.CountAsync(s => s.ArticleId == articleId)).Should().Be(1);
    }

    [Fact]
    public async Task InteractWithoutAuth_ShouldReturnUnauthorized()
    {
        Guid articleId = await SeedPublishedArticleAsync();

        Client.ClearAuthentication();

        HttpResponseMessage response = await Client.PostAsync(Routes.Public.Articles.Likes(articleId), null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Seeds a published article and returns its ID.
    /// </summary>
    private async Task<Guid> SeedPublishedArticleAsync()
    {
        ContentTypeEntity contentType = await SeedAsync<ContentDbContext, ContentTypeEntity>(context =>
        {
            ContentTypeEntity entity = ContentTypeFactory.Create("Article");
            context.ContentTypes.Add(entity);
            return entity;
        });

        CategoryEntity category = await SeedAsync<ContentDbContext, CategoryEntity>(context =>
        {
            CategoryEntity entity = CategoryFactory.Create(contentType.Id);
            context.Categories.Add(entity);
            return entity;
        });

        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(context =>
        {
            ArticleEntity entity = ArticleFactory.CreatePublished(category.Id);
            context.Articles.Add(entity);
            return entity;
        });

        return article.Id;
    }

    /// <summary>
    /// Seeds a published video and returns its ID.
    /// </summary>
    private async Task<Guid> SeedPublishedVideoAsync()
    {
        ContentTypeEntity contentType = await SeedAsync<ContentDbContext, ContentTypeEntity>(context =>
        {
            ContentTypeEntity entity = ContentTypeFactory.Create("Video");
            context.ContentTypes.Add(entity);
            return entity;
        });

        CategoryEntity category = await SeedAsync<ContentDbContext, CategoryEntity>(context =>
        {
            CategoryEntity entity = CategoryFactory.Create(contentType.Id);
            context.Categories.Add(entity);
            return entity;
        });

        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(context =>
        {
            VideoEntity entity = VideoFactory.CreatePublished(category.Id);
            context.Videos.Add(entity);
            return entity;
        });

        return video.Id;
    }
}
