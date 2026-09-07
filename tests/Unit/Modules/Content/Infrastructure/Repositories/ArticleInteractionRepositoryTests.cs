using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="ArticleInteractionRepository"/> using InMemory database.
/// </summary>
public class ArticleInteractionRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly ArticleInteractionRepository _repository;

    public ArticleInteractionRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new ArticleInteractionRepository(_context);
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

    #region HasLikedAsync Tests

    [Fact]
    public async Task HasLikedAsync_WhenLikeExists_ShouldReturnTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        ArticleLikeEntity like = ArticleLikeEntity.Create(Guid.NewGuid(), userId, article.Id);
        _context.ArticleLikes.Add(like);
        await _context.SaveChangesAsync();

        // Act
        bool result = await _repository.HasLikedAsync(userId, article.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasLikedAsync_WhenLikeDoesNotExist_ShouldReturnFalse()
    {
        // Act
        bool result = await _repository.HasLikedAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region AddLikeAsync Tests

    [Fact]
    public async Task AddLikeAsync_ShouldAddLikeToContext()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        ArticleLikeEntity like = ArticleLikeEntity.Create(Guid.NewGuid(), userId, article.Id);

        // Act
        await _repository.AddLikeAsync(like);
        await _context.SaveChangesAsync();

        // Assert
        bool exists = await _context.ArticleLikes.AnyAsync(l => l.UserId == userId && l.ArticleId == article.Id);
        exists.Should().BeTrue();
    }

    #endregion

    #region RemoveLikeAsync Tests

    [Fact]
    public async Task RemoveLikeAsync_WhenLikeExists_ShouldRemoveIt()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        ArticleLikeEntity like = ArticleLikeEntity.Create(Guid.NewGuid(), userId, article.Id);
        _context.ArticleLikes.Add(like);
        await _context.SaveChangesAsync();

        // Act
        await _repository.RemoveLikeAsync(userId, article.Id);
        await _context.SaveChangesAsync();

        // Assert
        bool exists = await _context.ArticleLikes.AnyAsync(l => l.UserId == userId && l.ArticleId == article.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveLikeAsync_WhenLikeDoesNotExist_ShouldNotThrow()
    {
        // Act
        Func<Task> act = async () => await _repository.RemoveLikeAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region HasBookmarkedAsync Tests

    [Fact]
    public async Task HasBookmarkedAsync_WhenBookmarkExists_ShouldReturnTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        ArticleBookmarkEntity bookmark = ArticleBookmarkEntity.Create(Guid.NewGuid(), userId, article.Id);
        _context.ArticleBookmarks.Add(bookmark);
        await _context.SaveChangesAsync();

        // Act
        bool result = await _repository.HasBookmarkedAsync(userId, article.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasBookmarkedAsync_WhenBookmarkDoesNotExist_ShouldReturnFalse()
    {
        // Act
        bool result = await _repository.HasBookmarkedAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region AddBookmarkAsync Tests

    [Fact]
    public async Task AddBookmarkAsync_ShouldAddBookmarkToContext()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        ArticleBookmarkEntity bookmark = ArticleBookmarkEntity.Create(Guid.NewGuid(), userId, article.Id);

        // Act
        await _repository.AddBookmarkAsync(bookmark);
        await _context.SaveChangesAsync();

        // Assert
        bool exists = await _context.ArticleBookmarks.AnyAsync(b => b.UserId == userId && b.ArticleId == article.Id);
        exists.Should().BeTrue();
    }

    #endregion

    #region RemoveBookmarkAsync Tests

    [Fact]
    public async Task RemoveBookmarkAsync_WhenBookmarkExists_ShouldRemoveIt()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        ArticleBookmarkEntity bookmark = ArticleBookmarkEntity.Create(Guid.NewGuid(), userId, article.Id);
        _context.ArticleBookmarks.Add(bookmark);
        await _context.SaveChangesAsync();

        // Act
        await _repository.RemoveBookmarkAsync(userId, article.Id);
        await _context.SaveChangesAsync();

        // Assert
        bool exists = await _context.ArticleBookmarks.AnyAsync(b => b.UserId == userId && b.ArticleId == article.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveBookmarkAsync_WhenBookmarkDoesNotExist_ShouldNotThrow()
    {
        // Act
        Func<Task> act = async () => await _repository.RemoveBookmarkAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region GetLikedAndBookmarkedIdsAsync Tests

    [Fact]
    public async Task GetLikedAndBookmarkedIdsAsync_ShouldReturnOnlyIdsTheUserInteractedWith()
    {
        // Arrange — user liked first and third, bookmarked second; another user liked/bookmarked the rest
        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        Guid categoryId = await SeedCategoryAsync();
        List<ArticleEntity> articles = ArticleFactory.CreateManyPublished(categoryId, 3);
        _context.Articles.AddRange(articles);
        _context.ArticleLikes.AddRange(
            ArticleLikeEntity.Create(Guid.NewGuid(), userId, articles[0].Id),
            ArticleLikeEntity.Create(Guid.NewGuid(), otherUserId, articles[1].Id),
            ArticleLikeEntity.Create(Guid.NewGuid(), userId, articles[2].Id)
        );
        _context.ArticleBookmarks.AddRange(
            ArticleBookmarkEntity.Create(Guid.NewGuid(), userId, articles[1].Id),
            ArticleBookmarkEntity.Create(Guid.NewGuid(), otherUserId, articles[0].Id)
        );
        await _context.SaveChangesAsync();

        // Act
        (IReadOnlySet<Guid> liked, IReadOnlySet<Guid> bookmarked) = await _repository.GetLikedAndBookmarkedIdsAsync(
            userId,
            [articles[0].Id, articles[1].Id, articles[2].Id]
        );

        // Assert
        liked.Should().BeEquivalentTo([articles[0].Id, articles[2].Id]);
        liked.Should().NotContain(articles[1].Id);
        bookmarked.Should().BeEquivalentTo([articles[1].Id]);
        bookmarked.Should().NotContain(articles[0].Id);
    }

    [Fact]
    public async Task GetLikedAndBookmarkedIdsAsync_ShouldNotReturnIdsOutsideTheInputList()
    {
        // Arrange — user liked and bookmarked both, but only the first is in the candidate list
        Guid userId = Guid.NewGuid();
        Guid categoryId = await SeedCategoryAsync();
        List<ArticleEntity> articles = ArticleFactory.CreateManyPublished(categoryId, 2);
        _context.Articles.AddRange(articles);
        _context.ArticleLikes.AddRange(
            ArticleLikeEntity.Create(Guid.NewGuid(), userId, articles[0].Id),
            ArticleLikeEntity.Create(Guid.NewGuid(), userId, articles[1].Id)
        );
        _context.ArticleBookmarks.AddRange(
            ArticleBookmarkEntity.Create(Guid.NewGuid(), userId, articles[0].Id),
            ArticleBookmarkEntity.Create(Guid.NewGuid(), userId, articles[1].Id)
        );
        await _context.SaveChangesAsync();

        // Act
        (IReadOnlySet<Guid> liked, IReadOnlySet<Guid> bookmarked) = await _repository.GetLikedAndBookmarkedIdsAsync(
            userId,
            [articles[0].Id]
        );

        // Assert
        liked.Should().BeEquivalentTo([articles[0].Id]);
        bookmarked.Should().BeEquivalentTo([articles[0].Id]);
    }

    [Fact]
    public async Task GetLikedAndBookmarkedIdsAsync_WithNullUser_ShouldReturnEmptySets()
    {
        // Arrange — the article has interactions from other users
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.CreatePublished(categoryId);
        _context.Articles.Add(article);
        _context.ArticleLikes.Add(ArticleLikeEntity.Create(Guid.NewGuid(), Guid.NewGuid(), article.Id));
        await _context.SaveChangesAsync();

        // Act
        (IReadOnlySet<Guid> liked, IReadOnlySet<Guid> bookmarked) = await _repository.GetLikedAndBookmarkedIdsAsync(
            currentUserId: null,
            articleIds: [article.Id]
        );

        // Assert
        liked.Should().BeEmpty();
        bookmarked.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLikedAndBookmarkedIdsAsync_WithEmptyInput_ShouldReturnEmptySets()
    {
        // Act
        (IReadOnlySet<Guid> liked, IReadOnlySet<Guid> bookmarked) = await _repository.GetLikedAndBookmarkedIdsAsync(
            Guid.NewGuid(),
            []
        );

        // Assert
        liked.Should().BeEmpty();
        bookmarked.Should().BeEmpty();
    }

    #endregion

    #region AddShareAsync Tests

    [Fact]
    public async Task AddShareAsync_ShouldAddShareToContext()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        ArticleShareEntity share = ArticleShareEntity.Create(Guid.NewGuid(), null, article.Id);

        // Act
        await _repository.AddShareAsync(share);
        await _context.SaveChangesAsync();

        // Assert
        bool exists = await _context.ArticleShares.AnyAsync(s => s.ArticleId == article.Id);
        exists.Should().BeTrue();
    }

    #endregion

    #region Article favorite read Tests

    [Fact]
    public async Task GetBookmarkedArticlesAsync_ReturnsBookmarkTimestampAndPublishedArticlesOnly()
    {
        Guid userId = Guid.NewGuid();
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity published = ArticleFactory.CreatePublished(categoryId);
        ArticleEntity draft = ArticleFactory.Create(categoryId);
        DateTime bookmarkedAt = DateTime.UtcNow.AddDays(-3);
        ArticleBookmarkEntity bookmark = ArticleBookmarkEntity.Create(Guid.NewGuid(), userId, published.Id);
        bookmark.CreatedAt = bookmarkedAt;
        _context.Articles.AddRange(published, draft);
        _context.ArticleBookmarks.AddRange(bookmark, ArticleBookmarkEntity.Create(Guid.NewGuid(), userId, draft.Id));
        await _context.SaveChangesAsync();

        (List<BookmarkedArticleActivity> activities, int count) = await _repository.GetBookmarkedArticlesAsync(
            userId,
            1,
            10
        );

        count.Should().Be(1);
        activities.Should().ContainSingle();
        activities.Single().Article.Id.Should().Be(published.Id);
        activities.Single().BookmarkedAt.Should().Be(bookmarkedAt);
    }

    [Fact]
    public async Task GetSharedArticlesAsync_GroupsOnlyCurrentUsersShares()
    {
        Guid userId = Guid.NewGuid();
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.CreatePublished(categoryId);
        ArticleShareEntity older = ArticleShareEntity.Create(
            Guid.NewGuid(),
            userId,
            article.Id,
            EnumShareChannel.Facebook
        );
        older.CreatedAt = DateTime.UtcNow.AddHours(-2);
        ArticleShareEntity latest = ArticleShareEntity.Create(
            Guid.NewGuid(),
            userId,
            article.Id,
            EnumShareChannel.WhatsApp
        );
        latest.CreatedAt = DateTime.UtcNow.AddHours(-1);
        _context.Articles.Add(article);
        _context.ArticleShares.AddRange(
            older,
            latest,
            ArticleShareEntity.Create(Guid.NewGuid(), Guid.NewGuid(), article.Id),
            ArticleShareEntity.Create(Guid.NewGuid(), null, article.Id)
        );
        await _context.SaveChangesAsync();

        (List<ArticleActivity> activities, int count) = await _repository.GetSharedArticlesAsync(userId, 1, 10);

        count.Should().Be(1);
        activities.Single().InteractionCount.Should().Be(2);
        activities.Single().LastInteractedAt.Should().Be(latest.CreatedAt);
        activities.Single().LastShareChannel.Should().Be(EnumShareChannel.WhatsApp);
    }

    #endregion
}
