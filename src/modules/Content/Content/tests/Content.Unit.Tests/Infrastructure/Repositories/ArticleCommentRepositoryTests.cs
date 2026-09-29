using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Tests.TestData.Constants;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Content.Unit.Tests.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="ArticleCommentRepository"/> using InMemory database.
/// </summary>
public class ArticleCommentRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly ArticleCommentRepository _repository;

    public ArticleCommentRepositoryTests()
    {
        _context = ContentDbContextFactory.CreateInMemory();
        _repository = new ArticleCommentRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Seeds a ContentType and Category, returning the Category ID.
    /// </summary>
    private async Task<Guid> SeedCategoryAsync()
    {
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        _context.ContentTypes.Add(contentType);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category.Id;
    }

    #region UpdateComment Tests

    [Fact]
    public async Task UpdateComment_ShouldMarkCommentAsModified()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        ArticleCommentEntity comment = ArticleCommentFactory.Create(article.Id, Guid.NewGuid(), "Test comment body");
        _context.ArticleComments.Add(comment);
        await _context.SaveChangesAsync();

        // Act — tracked mutation: editing the loaded entity is all a caller does now.
        comment.Edit("Edited comment body");
        await _context.SaveChangesAsync();

        // Assert
        (await _context.ArticleComments.FindAsync(comment.Id))!
            .Body.Should()
            .Be("Edited comment body");
    }

    #endregion

    #region AddCommentAsync Tests

    [Fact]
    public async Task AddCommentAsync_ShouldAddCommentToContext()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        ArticleCommentEntity comment = ArticleCommentFactory.Create(article.Id, Guid.NewGuid(), "Hello");

        // Act
        await _repository.AddCommentAsync(comment);
        await _context.SaveChangesAsync();

        // Assert
        bool exists = await _context.ArticleComments.AnyAsync(c => c.Id == comment.Id);
        exists.Should().BeTrue();
    }

    #endregion

    #region GetCommentsAsync Tests

    [Fact]
    public async Task GetCommentsAsync_WhenCommentsExist_ShouldReturnPaginatedComments()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);

        _context.ArticleComments.AddRange(
            ArticleCommentFactory.Create(article.Id, Guid.NewGuid(), "Comment 1"),
            ArticleCommentFactory.Create(article.Id, Guid.NewGuid(), "Comment 2")
        );
        await _context.SaveChangesAsync();

        // Act
        var (comments, totalCount) = await _repository.GetCommentsAsync(article.Id, page: 1, pageSize: 10);

        // Assert
        comments.Should().HaveCount(2);
        totalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetCommentsAsync_WhenNoComments_ShouldReturnEmpty()
    {
        // Act
        var (comments, totalCount) = await _repository.GetCommentsAsync(Guid.NewGuid(), page: 1, pageSize: 10);

        // Assert
        comments.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    #endregion

    #region GetCommentByIdAsync Tests

    [Fact]
    public async Task GetCommentByIdAsync_WhenCommentExists_ShouldReturnComment()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        ArticleCommentEntity comment = ArticleCommentFactory.Create(article.Id, Guid.NewGuid(), "Test");
        _context.ArticleComments.Add(comment);
        await _context.SaveChangesAsync();

        // Act
        ArticleCommentEntity? result = await _repository.GetCommentByIdAsync(comment.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(comment.Id);
    }

    [Fact]
    public async Task GetCommentByIdAsync_WhenCommentDoesNotExist_ShouldReturnNull()
    {
        // Act
        ArticleCommentEntity? result = await _repository.GetCommentByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Comment threading Tests

    [Fact]
    public async Task GetCommentsAsync_ShouldReturnOnlyTopLevelComments()
    {
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        ArticleCommentEntity top = ArticleCommentFactory.Create(article.Id, Guid.NewGuid(), "top");
        ArticleCommentEntity reply = ArticleCommentFactory.CreateReply(article.Id, Guid.NewGuid(), top.Id, "reply");
        _context.ArticleComments.AddRange(top, reply);
        await _context.SaveChangesAsync();

        var (comments, totalCount) = await _repository.GetCommentsAsync(article.Id, page: 1, pageSize: 10);

        comments.Should().ContainSingle(c => c.Id == top.Id);
        comments.Should().NotContain(c => c.Id == reply.Id);
        totalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetRepliesAsync_ShouldReturnOnlyNonDeletedRepliesOfTheParent()
    {
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        ArticleCommentEntity top = ArticleCommentFactory.Create(article.Id, Guid.NewGuid(), "top");
        ArticleCommentEntity reply1 = ArticleCommentFactory.CreateReply(article.Id, Guid.NewGuid(), top.Id, "r1");
        ArticleCommentEntity reply2 = ArticleCommentFactory.CreateReply(article.Id, Guid.NewGuid(), top.Id, "r2");
        reply2.SoftDelete(TestConstants.Clock.Instant);
        _context.ArticleComments.AddRange(top, reply1, reply2);
        await _context.SaveChangesAsync();

        var (replies, totalCount) = await _repository.GetRepliesAsync(top.Id, page: 1, pageSize: 10);

        replies.Should().ContainSingle(c => c.Id == reply1.Id);
        totalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetReplyCountsAsync_ShouldCountNonDeletedRepliesPerParent()
    {
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        ArticleCommentEntity parentA = ArticleCommentFactory.Create(article.Id, Guid.NewGuid(), "A");
        ArticleCommentEntity parentB = ArticleCommentFactory.Create(article.Id, Guid.NewGuid(), "B");
        ArticleCommentEntity a1 = ArticleCommentFactory.CreateReply(article.Id, Guid.NewGuid(), parentA.Id, "a1");
        ArticleCommentEntity a2 = ArticleCommentFactory.CreateReply(article.Id, Guid.NewGuid(), parentA.Id, "a2");
        _context.ArticleComments.AddRange(parentA, parentB, a1, a2);
        await _context.SaveChangesAsync();

        IReadOnlyDictionary<Guid, int> counts = await _repository.GetReplyCountsAsync([parentA.Id, parentB.Id]);

        counts.GetValueOrDefault(parentA.Id).Should().Be(2);
        counts.Should().NotContainKey(parentB.Id);
    }

    #endregion

    [Fact]
    public async Task GetCommentedArticlesAsync_GroupsRepliesAndExcludesDeletedAndOtherUsers()
    {
        Guid userId = Guid.NewGuid();
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.CreatePublished(categoryId);
        ArticleCommentEntity parent = ArticleCommentFactory.Create(article.Id, userId, "parent");
        parent.CreatedAt = DateTime.UtcNow.AddHours(-2);
        ArticleCommentEntity reply = ArticleCommentFactory.CreateReply(article.Id, userId, parent.Id, "reply");
        reply.CreatedAt = DateTime.UtcNow.AddHours(-1);
        ArticleCommentEntity deleted = ArticleCommentFactory.Create(article.Id, userId, "deleted");
        deleted.SoftDelete(TestConstants.Clock.Instant);
        ArticleCommentEntity other = ArticleCommentFactory.Create(article.Id, Guid.NewGuid(), "other");
        _context.Articles.Add(article);
        _context.ArticleComments.AddRange(parent, reply, deleted, other);
        await _context.SaveChangesAsync();

        (List<CommentedArticleActivity> activities, int count) = await _repository.GetCommentedArticlesAsync(
            userId,
            1,
            10
        );

        count.Should().Be(1);
        activities.Single().CommentCount.Should().Be(2);
        activities.Single().LatestComment.Id.Should().Be(reply.Id);
        activities.Single().LastCommentedAt.Should().Be(reply.CreatedAt);
    }

    [Fact]
    public async Task GetOwnCommentsForArticleAsync_ReturnsOnlyRemainingOwnCommentsWithPagination()
    {
        Guid userId = Guid.NewGuid();
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.CreatePublished(categoryId);
        ArticleCommentEntity first = ArticleCommentFactory.Create(article.Id, userId, "first");
        first.CreatedAt = DateTime.UtcNow.AddHours(-2);
        ArticleCommentEntity second = ArticleCommentFactory.Create(article.Id, userId, "second");
        second.CreatedAt = DateTime.UtcNow.AddHours(-1);
        ArticleCommentEntity deleted = ArticleCommentFactory.Create(article.Id, userId, "deleted");
        deleted.SoftDelete(TestConstants.Clock.Instant);
        _context.Articles.Add(article);
        _context.ArticleComments.AddRange(
            first,
            second,
            deleted,
            ArticleCommentFactory.Create(article.Id, Guid.NewGuid(), "other")
        );
        await _context.SaveChangesAsync();

        (List<ArticleCommentEntity> comments, int count) = await _repository.GetOwnCommentsForArticleAsync(
            userId,
            article.Id,
            1,
            1
        );

        count.Should().Be(2);
        comments.Should().ContainSingle();
        comments.Single().Id.Should().Be(second.Id);
    }

    #region Comment like Tests

    [Fact]
    public async Task HasLikedCommentAsync_WhenLikeExists_ReturnsTrue()
    {
        Guid userId = Guid.NewGuid();
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        ArticleCommentEntity comment = ArticleCommentFactory.Create(article.Id, Guid.NewGuid(), "c");
        _context.ArticleComments.Add(comment);
        _context.ArticleCommentLikes.Add(ArticleCommentLikeFactory.Create(userId, comment.Id));
        await _context.SaveChangesAsync();

        (await _repository.HasLikedCommentAsync(userId, comment.Id)).Should().BeTrue();
        (await _repository.HasLikedCommentAsync(Guid.NewGuid(), comment.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task GetLikedCommentIdsAsync_ShouldReturnOnlyIdsTheViewerLiked()
    {
        Guid viewerId = Guid.NewGuid();
        Guid otherId = Guid.NewGuid();
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        ArticleCommentEntity c1 = ArticleCommentFactory.Create(article.Id, Guid.NewGuid(), "c1");
        ArticleCommentEntity c2 = ArticleCommentFactory.Create(article.Id, Guid.NewGuid(), "c2");
        _context.ArticleComments.AddRange(c1, c2);
        _context.ArticleCommentLikes.AddRange(
            ArticleCommentLikeFactory.Create(viewerId, c1.Id),
            ArticleCommentLikeFactory.Create(otherId, c2.Id)
        );
        await _context.SaveChangesAsync();

        IReadOnlySet<Guid> liked = await _repository.GetLikedCommentIdsAsync(viewerId, [c1.Id, c2.Id]);

        liked.Should().BeEquivalentTo([c1.Id]);
    }

    [Fact]
    public async Task RemoveCommentLikeAsync_WhenLikeExists_RemovesIt()
    {
        Guid userId = Guid.NewGuid();
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        ArticleCommentEntity comment = ArticleCommentFactory.Create(article.Id, Guid.NewGuid(), "c");
        _context.ArticleComments.Add(comment);
        _context.ArticleCommentLikes.Add(ArticleCommentLikeFactory.Create(userId, comment.Id));
        await _context.SaveChangesAsync();

        await _repository.RemoveCommentLikeAsync(userId, comment.Id);
        await _context.SaveChangesAsync();

        (await _context.ArticleCommentLikes.AnyAsync(l => l.UserId == userId && l.CommentId == comment.Id))
            .Should()
            .BeFalse();
    }

    #endregion
}
