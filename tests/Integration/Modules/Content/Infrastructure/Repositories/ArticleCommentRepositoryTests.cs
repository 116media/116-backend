using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IArticleCommentRepository" /> verifying that soft-deleted
/// comments stay invisible to the reply counts, the own-comments listing and the commented
/// articles activity feed, and that comment likes are scoped to the viewer.
/// </summary>
[Collection("Database")]
public class ArticleCommentRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task GetReplyCountsAsync_WithSoftDeletedReply_CountsOnlyVisibleReplies()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        ArticleEntity article = await SeedPublishedArticleAsync(context);
        Guid userId = Guid.NewGuid();

        ArticleCommentEntity parent = ArticleCommentFactory.Create(article.Id, userId);
        context.ArticleComments.Add(parent);
        await context.SaveChangesAsync();

        ArticleCommentEntity visibleReply = CreateReply(article.Id, userId, parent.Id);
        ArticleCommentEntity deletedReply = CreateReply(article.Id, userId, parent.Id);
        deletedReply.SoftDelete(TestConstants.Clock.Instant);
        context.ArticleComments.AddRange(visibleReply, deletedReply);
        await context.SaveChangesAsync();

        var repository = Resolve<IArticleCommentRepository>();

        IReadOnlyDictionary<Guid, int> counts = await repository.GetReplyCountsAsync([parent.Id]);

        counts[parent.Id].Should().Be(1);
    }

    [Fact]
    public async Task GetOwnCommentsForArticleAsync_WithSoftDeletedComment_ExcludesIt()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        ArticleEntity article = await SeedPublishedArticleAsync(context);
        Guid userId = Guid.NewGuid();

        ArticleCommentEntity visible = ArticleCommentFactory.Create(article.Id, userId);
        ArticleCommentEntity deleted = ArticleCommentFactory.CreateDeleted(article.Id, userId);
        context.ArticleComments.AddRange(visible, deleted);
        await context.SaveChangesAsync();

        var repository = Resolve<IArticleCommentRepository>();

        (List<ArticleCommentEntity> comments, int totalCount) = await repository.GetOwnCommentsForArticleAsync(
            userId: userId,
            articleId: article.Id,
            page: 1,
            pageSize: 10
        );

        totalCount.Should().Be(1);
        comments.Should().ContainSingle(comment => comment.Id == visible.Id);
    }

    [Fact]
    public async Task GetCommentedArticlesAsync_WithSoftDeletedComment_ExcludesItFromTheCount()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        ArticleEntity article = await SeedPublishedArticleAsync(context);
        Guid userId = Guid.NewGuid();

        ArticleCommentEntity visible = ArticleCommentFactory.Create(article.Id, userId);
        ArticleCommentEntity deleted = ArticleCommentFactory.CreateDeleted(article.Id, userId);
        context.ArticleComments.AddRange(visible, deleted);
        await context.SaveChangesAsync();

        var repository = Resolve<IArticleCommentRepository>();

        (List<CommentedArticleActivity> activities, int totalCount) = await repository.GetCommentedArticlesAsync(
            userId: userId,
            page: 1,
            pageSize: 10
        );

        totalCount.Should().Be(1);
        activities.Should().ContainSingle();
        activities[0].CommentCount.Should().Be(1);
        activities[0].LatestComment.Id.Should().Be(visible.Id);
    }

    [Fact]
    public async Task GetCommentedArticlesAsync_WithOnlySoftDeletedComments_ReturnsNoActivity()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        ArticleEntity article = await SeedPublishedArticleAsync(context);
        Guid userId = Guid.NewGuid();

        context.ArticleComments.Add(ArticleCommentFactory.CreateDeleted(article.Id, userId));
        await context.SaveChangesAsync();

        var repository = Resolve<IArticleCommentRepository>();

        (List<CommentedArticleActivity> activities, int totalCount) = await repository.GetCommentedArticlesAsync(
            userId: userId,
            page: 1,
            pageSize: 10
        );

        totalCount.Should().Be(0);
        activities.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLikedCommentIdsAsync_ReturnsOnlyTheViewersLikes()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        ArticleEntity article = await SeedPublishedArticleAsync(context);
        Guid viewerUserId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();

        ArticleCommentEntity liked = ArticleCommentFactory.Create(article.Id, viewerUserId);
        ArticleCommentEntity likedByOther = ArticleCommentFactory.Create(article.Id, otherUserId);
        context.ArticleComments.AddRange(liked, likedByOther);
        await context.SaveChangesAsync();

        context.ArticleCommentLikes.AddRange(
            ArticleCommentLikeEntity.Create(id: Guid.NewGuid(), userId: viewerUserId, commentId: liked.Id),
            ArticleCommentLikeEntity.Create(id: Guid.NewGuid(), userId: otherUserId, commentId: likedByOther.Id)
        );
        await context.SaveChangesAsync();

        var repository = Resolve<IArticleCommentRepository>();

        IReadOnlySet<Guid> likedIds = await repository.GetLikedCommentIdsAsync(
            viewerUserId: viewerUserId,
            commentIds: [liked.Id, likedByOther.Id]
        );

        likedIds.Should().BeEquivalentTo([liked.Id]);
    }

    private static ArticleCommentEntity CreateReply(Guid articleId, Guid userId, Guid parentCommentId) =>
        ArticleCommentEntity.CreateReply(
            id: Guid.NewGuid(),
            userId: userId,
            articleId: articleId,
            parentCommentId: parentCommentId,
            body: TestConstants.Interactions.ValidCommentBody
        );

    private static async Task<ArticleEntity> SeedPublishedArticleAsync(ContentDbContext context)
    {
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        ArticleEntity article = ArticleFactory.CreatePublished(category.Id);
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        return article;
    }
}
