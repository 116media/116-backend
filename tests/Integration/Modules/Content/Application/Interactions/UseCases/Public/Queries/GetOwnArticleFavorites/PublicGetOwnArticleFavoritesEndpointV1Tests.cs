using _116.Content.Application.Interactions.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetOwnArticleFavorites;

/// <summary>
/// HTTP integration tests for current-user liked, commented, shared, and own-comment reads.
/// </summary>
[Collection("Database")]
public class PublicGetOwnArticleFavoritesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string LikedUrl => $"{ApiRoutes.Public.Articles}/{InteractionsRouteConstants.Liked}";
    private static string CommentedUrl => $"{ApiRoutes.Public.Articles}/{InteractionsRouteConstants.Commented}";
    private static string SharedUrl => $"{ApiRoutes.Public.Articles}/{InteractionsRouteConstants.Shared}";

    private static string MineUrl(Guid articleId) =>
        $"{ApiRoutes.Public.Articles}/{articleId}/{InteractionsRouteConstants.Comments}/{InteractionsRouteConstants.Me}";

    private async Task<ArticleEntity> SeedArticleAsync(bool published = true)
    {
        return await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            ArticleEntity article = published
                ? ArticleFactory.CreatePublished(category.Id)
                : ArticleFactory.Create(category.Id);
            ctx.Articles.Add(article);
            return article;
        });
    }

    [Theory]
    [MemberData(nameof(PrivateCollectionUrls))]
    public async Task PrivateArticleFavoriteReads_WithoutAuthentication_ReturnUnauthorized(string url)
    {
        Client.ClearAuthentication();

        HttpResponseMessage response = await Client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public static TheoryData<string> PrivateCollectionUrls => new() { LikedUrl, CommentedUrl, SharedUrl };

    [Fact]
    public async Task PrivateArticleFavoriteStatus_WithoutAuthentication_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        HttpResponseMessage response = await Client.GetAsync(MineUrl(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LikedArticles_ReturnOnlyCurrentUserRows_AndPaginateDistinctArticles()
    {
        ArticleEntity first = await SeedArticleAsync();
        ArticleEntity second = await SeedArticleAsync();
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.ArticleLikes.AddRange(
                ArticleLikeEntity.Create(Guid.NewGuid(), TestUser.VisitorId, first.Id),
                ArticleLikeEntity.Create(Guid.NewGuid(), TestUser.VisitorId, second.Id),
                ArticleLikeEntity.Create(Guid.NewGuid(), TestUser.AdminId, first.Id)
            );
        });
        Client.AuthenticateAsVisitor();

        HttpResponseMessage response = await Client.GetAsync($"{LikedUrl}?pageIndex=0&pageSize=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PaginatedResult<UserArticleActivityDto> body = await response.ReadAsAsync<
            PaginatedResult<UserArticleActivityDto>
        >();
        body.Count.Should().Be(2);
        body.Items.Should().ContainSingle();
        body.Items.Single().Article.IsLiked.Should().BeTrue();
        body.Items.Single().LastInteractedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task SharedArticles_GroupCurrentUserEvents_AndIgnoreOtherAndAnonymousShares()
    {
        ArticleEntity article = await SeedArticleAsync();
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.ArticleShares.AddRange(
                ArticleShareEntity.Create(Guid.NewGuid(), TestUser.VisitorId, article.Id, EnumShareChannel.Facebook),
                ArticleShareEntity.Create(Guid.NewGuid(), TestUser.VisitorId, article.Id, EnumShareChannel.WhatsApp),
                ArticleShareEntity.Create(Guid.NewGuid(), TestUser.AdminId, article.Id, EnumShareChannel.X),
                ArticleShareEntity.Create(Guid.NewGuid(), null, article.Id, EnumShareChannel.Clipboard)
            );
        });
        Client.AuthenticateAsVisitor();

        HttpResponseMessage response = await Client.GetAsync(SharedUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PaginatedResult<UserArticleActivityDto> body = await response.ReadAsAsync<
            PaginatedResult<UserArticleActivityDto>
        >();
        body.Items.Should().ContainSingle();
        body.Items.Single().InteractionCount.Should().Be(2);
        body.Items.Single().LastInteractedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task CommentedArticles_IncludeReplies_ExcludeDeletedAndOtherUsers_AndGroupByArticle()
    {
        ArticleEntity article = await SeedArticleAsync();
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ArticleCommentEntity parent = ArticleCommentFactory.Create(article.Id, TestUser.VisitorId);
            ArticleCommentEntity reply = ArticleCommentEntity.CreateReply(
                Guid.NewGuid(),
                TestUser.VisitorId,
                article.Id,
                parent.Id,
                "Visitor reply"
            );
            ArticleCommentEntity deleted = ArticleCommentFactory.CreateDeleted(article.Id, TestUser.VisitorId);
            ArticleCommentEntity other = ArticleCommentFactory.Create(article.Id, TestUser.AdminId);
            ctx.ArticleComments.AddRange(parent, reply, deleted, other);
        });
        Client.AuthenticateAsVisitor();

        HttpResponseMessage response = await Client.GetAsync(CommentedUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PaginatedResult<UserCommentedArticleDto> body = await response.ReadAsAsync<
            PaginatedResult<UserCommentedArticleDto>
        >();
        body.Count.Should().Be(1);
        body.Items.Should().ContainSingle();
        body.Items.Single().CommentCount.Should().Be(2);
        body.Items.Single().LatestComment.UserId.Should().Be(TestUser.VisitorId);
        body.Items.Single().LatestComment.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task MyComments_ReturnOnlyCurrentUsersRemainingTopLevelCommentsAndReplies()
    {
        ArticleEntity article = await SeedArticleAsync();
        Guid parentId = Guid.NewGuid();
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ArticleCommentEntity parent = ArticleCommentEntity.Create(
                parentId,
                TestUser.VisitorId,
                article.Id,
                "Parent"
            );
            ctx.ArticleComments.AddRange(
                parent,
                ArticleCommentEntity.CreateReply(Guid.NewGuid(), TestUser.VisitorId, article.Id, parentId, "Reply"),
                ArticleCommentFactory.CreateDeleted(article.Id, TestUser.VisitorId),
                ArticleCommentFactory.Create(article.Id, TestUser.AdminId)
            );
        });
        Client.AuthenticateAsVisitor();

        HttpResponseMessage response = await Client.GetAsync(MineUrl(article.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PaginatedResult<ArticleCommentDto> body = await response.ReadAsAsync<PaginatedResult<ArticleCommentDto>>();
        body.Count.Should().Be(2);
        body.Items.Should().OnlyContain(comment => comment.UserId == TestUser.VisitorId && !comment.IsDeleted);
        body.Items.Should().Contain(comment => comment.ParentCommentId == parentId);
    }

    [Fact]
    public async Task MyComments_ForUnpublishedArticle_ReturnsNotFound()
    {
        ArticleEntity article = await SeedArticleAsync(published: false);
        Client.AuthenticateAsVisitor();

        HttpResponseMessage response = await Client.GetAsync(MineUrl(article.Id));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Article"))
        );
    }

    [Fact]
    public async Task DeletingLastOwnComment_RemovesArticleFromCommentedCollection()
    {
        ArticleEntity article = await SeedArticleAsync();
        ArticleCommentEntity comment = await SeedAsync<ContentDbContext, ArticleCommentEntity>(ctx =>
        {
            ArticleCommentEntity seeded = ArticleCommentFactory.Create(article.Id, TestUser.VisitorId);
            ctx.ArticleComments.Add(seeded);
            return seeded;
        });
        Client.AuthenticateAsVisitor();

        HttpResponseMessage deleteResponse = await Client.DeleteAsync(
            Routes.Public.Articles.Comment(article.Id, comment.Id)
        );
        HttpResponseMessage collectionResponse = await Client.GetAsync(CommentedUrl);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        PaginatedResult<UserCommentedArticleDto> body = await collectionResponse.ReadAsAsync<
            PaginatedResult<UserCommentedArticleDto>
        >();
        body.Count.Should().Be(0);
        body.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Collections_ExcludeUnpublishedParentArticles()
    {
        ArticleEntity article = await SeedArticleAsync(published: false);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.ArticleLikes.Add(ArticleLikeEntity.Create(Guid.NewGuid(), TestUser.VisitorId, article.Id));
            ctx.ArticleShares.Add(ArticleShareEntity.Create(Guid.NewGuid(), TestUser.VisitorId, article.Id));
            ctx.ArticleComments.Add(ArticleCommentFactory.Create(article.Id, TestUser.VisitorId));
        });
        Client.AuthenticateAsVisitor();

        var responses = await Task.WhenAll(
            Client.GetAsync(LikedUrl),
            Client.GetAsync(SharedUrl),
            Client.GetAsync(CommentedUrl)
        );

        foreach (HttpResponseMessage response in responses)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            string json = await response.Content.ReadAsStringAsync();
            json.Should().Contain("\"count\":0");
        }
    }
}
