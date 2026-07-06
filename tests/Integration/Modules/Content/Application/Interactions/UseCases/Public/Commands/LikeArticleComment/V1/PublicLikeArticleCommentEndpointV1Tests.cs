using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.LikeArticleComment.V1;

/// <summary>
/// Integration tests for the like / unlike article comment endpoints.
/// </summary>
[Collection("Database")]
public class PublicLikeArticleCommentEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<(ArticleEntity Article, ArticleCommentEntity Comment)> SeedArticleWithCommentAsync()
    {
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            ArticleEntity created = ArticleFactory.CreatePublished(category.Id);
            ctx.Articles.Add(created);
            return created;
        });

        ArticleCommentEntity comment = await SeedAsync<ContentDbContext, ArticleCommentEntity>(ctx =>
        {
            ArticleCommentEntity created = ArticleCommentFactory.Create(article.Id, TestUser.VisitorId);
            ctx.ArticleComments.Add(created);
            return created;
        });

        return (article, comment);
    }

    private async Task<int> GetLikeCountAsync(Guid commentId)
    {
        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArticleCommentEntity comment = await ctx.ArticleComments.FirstAsync(c => c.Id == commentId);
        return comment.LikeCount;
    }

    [Fact]
    public async Task LikeComment_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync(Routes.Public.Articles.CommentLike(Guid.NewGuid()), null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LikeThenUnlike_RoundTripsLikeCountAndIsLiked()
    {
        (ArticleEntity article, ArticleCommentEntity comment) = await SeedArticleWithCommentAsync();
        Client.AuthenticateAsVisitor();

        // Like
        var likeResponse = await Client.PostAsync(Routes.Public.Articles.CommentLike(comment.Id), null);
        likeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        (await GetLikeCountAsync(comment.Id)).Should().Be(1);

        // The comment list reflects isLiked=true and likeCount=1 for the viewer
        var listResponse = await Client.GetAsync(Routes.Public.Articles.Comments(article.Id));
        PaginatedResult<ArticleCommentDto> listed = await listResponse.ReadAsAsync<
            PaginatedResult<ArticleCommentDto>
        >();
        ArticleCommentDto dto = listed.Items.Single(c => c.Id == comment.Id);
        dto.IsLiked.Should().BeTrue();
        dto.LikeCount.Should().Be(1);

        // Unlike
        var unlikeResponse = await Client.DeleteAsync(Routes.Public.Articles.CommentLike(comment.Id));
        unlikeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        (await GetLikeCountAsync(comment.Id)).Should().Be(0);
    }

    [Fact]
    public async Task LikeComment_IsIdempotent()
    {
        (_, ArticleCommentEntity comment) = await SeedArticleWithCommentAsync();
        Client.AuthenticateAsVisitor();

        await Client.PostAsync(Routes.Public.Articles.CommentLike(comment.Id), null);
        var secondLike = await Client.PostAsync(Routes.Public.Articles.CommentLike(comment.Id), null);

        secondLike.StatusCode.Should().Be(HttpStatusCode.OK);
        (await GetLikeCountAsync(comment.Id)).Should().Be(1);
    }

    [Fact]
    public async Task AnotherUsersLike_DoesNotSetViewerIsLiked()
    {
        (ArticleEntity article, ArticleCommentEntity comment) = await SeedArticleWithCommentAsync();

        // A different user liked the comment.
        await SeedAsync<ContentDbContext, ArticleCommentLikeEntity>(ctx =>
        {
            ArticleCommentLikeEntity like = ArticleCommentLikeEntity.Create(Guid.NewGuid(), Guid.NewGuid(), comment.Id);
            ctx.ArticleCommentLikes.Add(like);
            return like;
        });

        // The visitor, who has not liked it, sees isLiked=false.
        Client.AuthenticateAsVisitor();
        var response = await Client.GetAsync(Routes.Public.Articles.Comments(article.Id));

        PaginatedResult<ArticleCommentDto> body = await response.ReadAsAsync<PaginatedResult<ArticleCommentDto>>();
        body.Items.Single(c => c.Id == comment.Id).IsLiked.Should().BeFalse();
    }
}
